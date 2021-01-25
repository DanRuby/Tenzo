using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.Recorder
{
    public enum DeviceStates {
        DllNotFound,
        DeviceNotFound,
        IncorrectPassword,
        InvalidData,
        DemoMode,
        AllRight,
        Undefined,
    }

    /// <summary>
    /// Устройство создается вызовом CreateDevice
    /// С помощью AddListener вешаются "слушатели на некий ID" 
    /// Удалить - Abort
    /// Start/stop - переход в режим ожидания, по умолчанию запущен
    /// DemoMode - возможность имитировать синус
    /// </summary>
    public class Device {
        public enum WorkMode {
            Normal,
            AdcCheck
        }
        public Action<bool[], short[]> AdcTestCallBack;
        public DeviceCounters Counters { get; set; }

        private static Dictionary<int, Device> Devices = new Dictionary<int, Device>();
        private bool isAbort = false;
        private bool isRun;
        private bool mDemoMode;

        private Collector Collector;
        private Dictionary<ushort, Action<ushort, Hand, Hand>> mCallBacks = new Dictionary<ushort, Action<ushort, Hand, Hand>>();
        private DeviceStates mDeviceState;
        private ushort? mLastPacketId = null;
        private object mLock = new object();
        private Task mRunTask;

        #region additional public fields
        public bool DemoMode {
            get { return mDemoMode; }
            set {
                mDemoMode = value;
                if( mDemoMode == true )
                    DeviceState = DeviceStates.DemoMode;
            }
        }

        public DeviceStates DeviceState {
            get { return mDeviceState; }
            private set {
                lock( mLock ) {
                    mDeviceState = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// Удаляет устройство
        /// </summary>
        public void Abort() {
            isAbort = true;
            mRunTask.Wait();
            mRunTask.Dispose();

            Collector.UnLoad();

            var dv = Devices.ToList().Where( device => device.Value.Equals( this ) );
            if( dv.Any() ) {
                var id = dv.ToArray()[0].Key;
                Devices.Remove( id );
            }
        }

        /// <summary>
        /// Удаляет все устройства
        /// </summary>
        public static void AbortAll() {
            while( Devices.Any() )
                Devices.First().Value.Abort();
            Devices.Clear();
        }

        /// <summary>
        /// Добавление слушателя на определенный ID
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="handCallBack"></param>
        public void AddListener( ushort requestId, Action<ushort, Hand, Hand> handCallBack ) {
            if( mCallBacks.ContainsKey( requestId ) ) {
                mCallBacks[requestId] += handCallBack;
            } else {
                mCallBacks.Add( requestId, handCallBack );
            }
        }

        /// <summary>
        /// Добавление слушателя на все пакеты
        /// </summary>
        /// <param name="handCallBack"></param>
        public void AddListener( Action<ushort, Hand, Hand> handCallBack ) => AddListener(0, handCallBack);

        /// <summary>
        /// Создание "усройства" с номером
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Device CreateDevice( int id ) {
            if( Devices.ContainsKey( id ) ) {
                return Devices[id];
            }
            Device device = new Device();
            Devices.Add( id, device );
            return device;
        }

        public static Device GetDevice( int id ) {
            if( Devices.ContainsKey( id ) ) {
                return Devices[id];
            }
            return CreateDevice( id );
        }

        public string GetDllName() => Collector.DllName;
        
        public bool IsDllLoad() =>Collector.IsDllLoad();

        /// <summary>
        /// Заявка на изменение ID поступающих измерений
        /// </summary>
        /// <param name="requestID"></param>
        /// <returns></returns>
        public bool NewRequest( ushort requestID ) {
            DeviceState = GetState();
            if( DeviceState == DeviceStates.DemoMode ) {
                Collector.FakeRequestId = requestID;
                return true;
            }
            if( DeviceState == DeviceStates.AllRight ) {
                Packet toDevice = new Packet();
                toDevice.Command = Commands.ToDevice.NEW_REQUEST;
                toDevice.RequestId = requestID;
                byte[] bytes = Packet.Packet2Bytes( toDevice );
                bool result = Collector.WriteData( bytes );
                return result;
            } else {
                return false;
            }
        }

        public void RemoveListener( Action<ushort, Hand, Hand> handCallBack ) => RemoveListener( 0, handCallBack );
        
        public void RemoveListener( ushort requestId, Action<ushort, Hand, Hand> handCallBack ) {
            if( mCallBacks.ContainsKey( requestId ) )
                mCallBacks[requestId] -= handCallBack;
        }

        public bool SetMode( WorkMode workMode ) {
            DeviceState = GetState();
            if( DeviceState == DeviceStates.DemoMode ) {
                return false;
            }
            if( DeviceState == DeviceStates.AllRight ) {
                byte cmd = workMode == WorkMode.AdcCheck ? Commands.ToDevice.SM_ADCCHECK : Commands.ToDevice.SM_NORMAL;
                Packet toDevice = new Packet();
                toDevice.Command = cmd;
                byte[] bytes = Packet.Packet2Bytes( toDevice );
                bool result = Collector.WriteData( bytes );
                return result;
            }
            return false;
        }

        public void Start() => isRun = true;
        
        public void Stop()=> isRun = false;

        /// <summary>
        /// Д:Получает байты от тензометра
        /// </summary>
        /// <returns></returns>
        internal byte[] GetBytes() {
            byte[] buffer = new byte[Packet.PACKET_SIZE];
            if( DemoMode == true ) {
                Collector.ReadFakeData( ref buffer );
            } else {
                Collector.ReadData( ref buffer );
            }
            return buffer;
        }

        private Device() {
            Counters = new DeviceCounters();
            Collector = new Collector();
            DeviceState = DeviceStates.Undefined;
            DemoMode = false;
            isRun = false;
            mRunTask = new TaskFactory().StartNew( Run );
        }

        private bool PacketIsValidAndNotaCopy( Packet packet ) {
            if( mLastPacketId == null ) {
                mLastPacketId = (ushort) (packet.PackId - 0x01);
            }
            // подходит по команде и имеет информацию
            if( packet.Command == Commands.FromDevice.DATA && packet.IsValid ) {
                Counters.FullPack ++;
                Counters.PPS.Increment();
                // точно новый
                if( packet.PackId != mLastPacketId ) {
                    // ничего не потеряли?
                    if( (ushort) (packet.PackId - mLastPacketId) != 1 ) {
                        Counters.LostPack += (ushort) (packet.PackId - mLastPacketId);
                    }
                    Counters.ValidPPS.Increment();
                    mLastPacketId = packet.PackId;
                    return true;
                } else {
                    Counters.RepeatPack++;
                }
            } else {
                Counters.InvalidPack++;
            }
            return false;
        }

        private DeviceStates GetState() {
            if( DemoMode == true ) {
                return DeviceStates.DemoMode;
            }
            if( Collector.IsDllConnect() == false ) {
                return DeviceStates.DllNotFound;
            }
            if( Collector.IsDeviceConnect() == false ) {
                Collector.InitUsb();
                return DeviceStates.DeviceNotFound;
            }
            return DeviceStates.AllRight;
        }

        private void Run() {
            while( true ) {
                if( isAbort == true ) {
                    break;
                }
                DeviceState = GetState();
                if( isRun == true ) {
                    if( DeviceState == DeviceStates.AllRight || DeviceState == DeviceStates.DemoMode ) {
                        Counters.Connections = 0;
                        Counters.TotalPack ++;
                        byte[] bytes = GetBytes();
                        Packet packet = Packet.Bytes2Packet( bytes );
                        //Д: с учетом закоменченных строк у меня ощущение что это все не нужно
                        //достаточно проверки пакета
                        if( packet.Command == Commands.FromDevice.DATA ) {
                            if( PacketIsValidAndNotaCopy( packet ) ) {
                                SendPacket( packet );
                            }
                        } else if( packet.Command == Commands.FromDevice.ADCCHECK ) {
                            //if( AdcTestCallBack != null )
                            //    AdcTestCallBack( pack.DataReadyM2, pack.ADCDataM2 );
                        } else {
                            Counters.InvalidPack ++;
                        }
                    } else {
                        Counters.Connections ++;
                        Thread.Sleep( 200 );
                    }
                } else Thread.Sleep( 200 );
            }
            return;
        }

        /// <summary>
        /// Отправляем обе руки каждому слушателю
        /// </summary>
        /// <param name="pack"></param>
        private void SendPacket( Packet pack ) {
            ushort id = pack.RequestId;
            if( id != 0 && mCallBacks.ContainsKey( id ) ) {
                if( mCallBacks[id] != null ) {
                    mCallBacks[id]( id, pack.Left, pack.Right );
                }
            }
            if( mCallBacks.ContainsKey( 0 ) ) {
                if( mCallBacks[0] != null ) {
                    mCallBacks[0]( id, pack.Left, pack.Right );
                }
            }
        }
    }

    /// <summary>
    /// Счетчики устройства по пакетам
    /// </summary>
    public class DeviceCounters {
        /// <summary>
        /// Попытки подключения
        /// </summary>
        public int Connections { get; set; }

        /// <summary>
        /// Принятые к обработке пакеты
        /// </summary>
        public int FullPack { get; internal set; }

        /// <summary>
        /// Битые пакеты
        /// </summary>
        public int InvalidPack { get; internal set; }

        /// <summary>
        /// Утеряно пакетов
        /// </summary>
        public int LostPack { get; internal set; }

        /// <summary>
        /// Всего пакетов в секунду, включая повторяющиеся
        /// </summary>
        public PerSeconds PPS { get; internal set; }

        /// <summary>
        /// Потеряно пакетов
        /// </summary>
        public int RepeatPack { get; internal set; }

        /// <summary>
        /// Получено пакетов
        /// </summary>
        public int TotalPack { get; internal set; }

        /// <summary>
        /// Принятые к запись пакеты в секунду
        /// </summary>
        public PerSeconds ValidPPS { get; internal set; }

        public DeviceCounters() {
            Clear();
        }

        public void Clear() {
            FullPack = 0;
            InvalidPack = 0;
            LostPack = 0;
            PPS = new PerSeconds();
            RepeatPack = 0;
            TotalPack = 0;
            ValidPPS = new PerSeconds();
        }
    }

    /// <summary>
    /// Счетчик пакетов в секунду
    /// </summary>
    public class PerSeconds {
        private object mLock = new object();
        private FixedSizedQueue<DateTime> mQueue = new FixedSizedQueue<DateTime>();

        public PerSeconds() {
            mQueue.Limit = 2;
            mQueue.Enqueue( DateTime.Now );
            mQueue.Enqueue( DateTime.Now );
        }

        /// <summary>
        /// Величина в секунду
        /// </summary>
        /// <returns></returns>
        public double GetPs() {
            lock( mLock ) {
                var time = (mQueue.Last() - mQueue.First()).TotalMilliseconds;
                // лимит корректируется во времени
                if( mQueue.Count == mQueue.Limit ) {
                    if( time < 500 )
                        mQueue.Limit ++;
                    if( time > 2000 )
                        mQueue.Limit --;
                }
                return (mQueue.Count/time)*1000;
            }
        }

        /// <summary>
        /// Увеличивает счетчик
        /// </summary>
        public void Increment() {
            lock( mLock ) {
                mQueue.Enqueue( DateTime.Now );
            }
        }
    }
}