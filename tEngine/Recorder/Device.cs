using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.Recorder {
    public enum DeviceStates {
        DllNotFind,
        DeviceNotFind,
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

        private static Dictionary<int, Device> Devices = new Dictionary<int, Device>();
        public Action<bool[], short[]> AdcTestCallBack;
        private Collector Collector;
        private bool isAbort = false;
        private bool isRun;

        private Dictionary<ushort, Action<ushort, Hand, Hand>> mCallBacks =
            new Dictionary<ushort, Action<ushort, Hand, Hand>>();

        private bool mDemoMode;
        private DeviceStates mDeviceState;
        private ushort? mLastId = null;
        private Packet mLastPacket = null;
        private object mLock = new object();
        private Task mRunTask;
        public DeviceCounters Counters { get; set; }

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
        public void AddListener( Action<ushort, Hand, Hand> handCallBack ) {
            AddListener( 0, handCallBack );
        }

        /// <summary>
        /// Создание "усройства" с номером
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Device CreateDevice( int id ) {
            if( Devices.ContainsKey( id ) ) {
                return Devices[id];
            }
            var device = new Device();
            Devices.Add( id, device );
            return device;
        }

        public static Device GetDevice( int id ) {
            if( Devices.ContainsKey( id ) ) {
                return Devices[id];
            }
            return CreateDevice( id );
        }

        public string GetDllName() {
            return Collector.DllName;
        }

        public bool IsDllLoad() {
            return Collector.IsDllLoad();
        }

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
                var toDevice = new Packet();
                toDevice.Command = Commands.ToDevice.NEW_REQUEST;
                toDevice.RequestId = requestID;
                var bytes = Packet.Packet2Bytes( toDevice );
                var result = Collector.WriteData( bytes );
                return result;
            } else {
                return false;
            }
        }

        public void RemoveListener( Action<ushort, Hand, Hand> handCallBack ) {
            RemoveListener( 0, handCallBack );
        }

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
                var cmd = workMode == WorkMode.AdcCheck ? Commands.ToDevice.SM_ADCCHECK : Commands.ToDevice.SM_NORMAL;
                var toDevice = new Packet();
                toDevice.Command = cmd;
                var bytes = Packet.Packet2Bytes( toDevice );
                var result = Collector.WriteData( bytes );
                return result;
            }
            return false;
        }

        public void Start() {
            isRun = true;
        }

        public void Stop() {
            isRun = false;
        }

        internal byte[] GetBytes() {
            var buffer = new byte[Packet.PACKET_SIZE];
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

        private bool FilterPack( Packet pack ) {
            if( mLastId == null ) {
                mLastId = (ushort) (pack.PackId - 0x01);
            }
            // подходит по команде и имеет информацию
            if( pack.Command == Commands.FromDevice.DATA && pack.IsValid ) {
                Counters.FullPack ++;
                Counters.PPS.Increment();
                // точно новый
                if( pack.PackId != mLastId ) {
                    // ничего не потеряли?
                    if( (ushort) (pack.PackId - mLastId) != 1 ) {
                        Counters.LostPack += (ushort) (pack.PackId - mLastId);
                    }
                    Counters.ValidPPS.Increment();
                    mLastId = pack.PackId;
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
                return DeviceStates.DllNotFind;
            }
            if( Collector.IsDeviceConnect() == false ) {
                Collector.InitUsb();
                return DeviceStates.DeviceNotFind;
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
                        var bytes = GetBytes();
                        var pack = Packet.Bytes2Packet( bytes );
                        if( pack.Command == Commands.FromDevice.DATA ) {
                            if( FilterPack( pack ) ) {
                                SendPacket( pack );
                            }
                        } else if( pack.Command == Commands.FromDevice.ADCCHECK ) {
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
            var id = pack.RequestId;
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
    /// не уверен что все счетчики задействованы
    /// todo проверить счетчики
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
            TotalPack = 0;
            FullPack = 0;
            InvalidPack = 0;
            LostPack = 0;
            RepeatPack = 0;
            PPS = new PerSeconds();
            ValidPPS = new PerSeconds();
        }
    }

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