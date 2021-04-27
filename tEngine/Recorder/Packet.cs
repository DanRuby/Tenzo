using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using tEngine.DataModel;

namespace tEngine.Recorder
{
    /// <summary>
    /// Инкапсуляция пакета данных прибора
    /// </summary>
    public class Packet
    {
        public const int ADC_COUNT = 4;
        public const int CYCLES = 7;
        public const int PACKET_SIZE = 64;
        private const int DATA_SIZE = 56;
        private const int HEADER_SIZE = 8;
        private PacketStruct Buffer;

        /// <summary>
        /// Структура пакета данных
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = HEADER_SIZE, Pack = 1)]
        public struct PacketStruct
        {
            public byte Command;
            public ushort PackID;
            public ushort RequestID;
            public byte NotUsed1;
            public byte NotUsed2;
            public byte ControlSum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DATA_SIZE)]
            public byte[] Data;
        }

        /// <summary>
        /// Комманда прибора 
        /// </summary>
        public byte Command
        {
            get => Buffer.Command;
            set => Buffer.Command = value;
        }

        /// <summary>
        /// Контрольная сумма пакета
        /// </summary>
        public byte ControlSum
        {
            get => Buffer.ControlSum;
            set => Buffer.ControlSum = value;
        }

        /// <summary>
        /// Идентификационный номер пакета
        /// </summary>
        public ushort PackId
        {
            get => Buffer.PackID;
            set => Buffer.PackID = value;
        }


        public ushort RequestId
        {
            get => Buffer.RequestID;
            set => Buffer.RequestID = value;
        }
        
        /// <summary>
        /// Пакет корректен
        /// </summary>
        public bool IsValid { get; set; }

        public HandRawData Left { get; set; }
        public HandRawData Right { get; set; }


        public Packet()
        {
            Left = new HandRawData();
            Right = new HandRawData();
            Buffer = new PacketStruct();
            Buffer.Data = new byte[DATA_SIZE];
        }

        /// <summary>
        /// Формирует пакет из байтов, полученных с юсб
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Packet BytesToPacket(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            PacketStruct buffer;
            try
            {
                buffer = (PacketStruct)Marshal.PtrToStructure(
                    handle.AddrOfPinnedObject(), typeof(PacketStruct));
            }
            finally
            {
                handle.Free();
            }

            Packet packet = new Packet();
            packet.Buffer = buffer;
            packet.IsValid = packet.CheckValid();
            if (packet.IsValid)
            {
                if (packet.Buffer.Command == Commands.FromDevice.DATA)
                {
                    packet.Left = new HandRawData();
                    packet.Right = new HandRawData();

                    short[] adcData = new short[ADC_COUNT * CYCLES];
                    System.Buffer.BlockCopy(packet.Buffer.Data, 0, adcData, 0, DATA_SIZE);
                    short[][] adc = new short[ADC_COUNT][];
                    for (int i = 0; i < ADC_COUNT; i++)
                    {
                        adc[i] = adcData.Where((s, j) => j >= CYCLES * i && j < CYCLES * (i + 1)).ToArray();
                    }

                    packet.Right.Constant.AddRange(adc[0]);
                    packet.Right.Tremor.AddRange(adc[1]);
                    packet.Left.Constant.AddRange(adc[2]);
                    packet.Left.Tremor.AddRange(adc[3]);
                }
                else if (packet.Buffer.Command == Commands.FromDevice.ADCCHECK) { }
            }
            return packet;
        }

      

        /// <summary>
        /// Трансформирует пакет в байты 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static byte[] PacketToBytes(Packet packet)
        {
            byte[] buffer = new byte[Marshal.SizeOf(packet.Buffer)];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IEnumerable<short> adc = packet.Right.Constant.AsEnumerable();
                adc = adc.Concat(packet.Right.Tremor);
                adc = adc.Concat(packet.Left.Constant);
                adc = adc.Concat(packet.Left.Tremor);
                IList<short> data = adc as IList<short> ?? adc.ToList();
                if (data.Count * sizeof(short) == DATA_SIZE)
                {
                    System.Buffer.BlockCopy(data.ToArray(), 0, packet.Buffer.Data, 0, DATA_SIZE);
                }
                packet.ControlSum = packet.CalculateControlSum();

                System.IntPtr pBuffer = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(packet.Buffer, pBuffer, false);
            }
            finally
            {
                handle.Free();
            }
            return buffer;
        }

        /// <summary>
        /// Проверить пакет на корректность
        /// </summary>
        /// <returns></returns>
        private bool CheckValid()
        {
            byte calculatedControlSum = CalculateControlSum();
            if (calculatedControlSum != ControlSum)
                return false;
            return true;
        }

        /// <summary>
        /// Вычисление контрольноый суммы
        /// </summary>
        /// <returns></returns>
        private byte CalculateControlSum()
        {
            byte sum = 0x00;
            for (int i = 0; i < DATA_SIZE; i++)
            {
                sum ^= Buffer.Data[i];
            }
            return sum;
        }

    
    }
}