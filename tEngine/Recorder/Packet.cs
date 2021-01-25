using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using tEngine.DataModel;

namespace tEngine.Recorder
{
    public class Packet
    {
        public const int ADC_COUNT = 4;
        public const int CYCLES = 7;
        public const int PACKET_SIZE = 64;
        private const int DATA_SIZE = 56;
        private const int HEADER_SIZE = 8;
        public PacketStruct Buffer;

        public byte Command
        {
            get { return Buffer.Command; }
            set { Buffer.Command = value; }
        }

        public byte ControlSum
        {
            get { return Buffer.ControlSum; }
            set { Buffer.ControlSum = value; }
        }

        public ushort PackId
        {
            get { return Buffer.PackID; }
            set { Buffer.PackID = value; }
        }

        public ushort RequestId
        {
            get { return Buffer.RequestID; }
            set { Buffer.RequestID = value; }
        }

        public bool IsValid { get; set; }
        public Hand Left { get; set; }
        public Hand Right { get; set; }


        public Packet()
        {
            Left = new Hand();
            Right = new Hand();
            Buffer = new PacketStruct();
            Buffer.Data = new byte[DATA_SIZE];
        }

        public static Packet Bytes2Packet(byte[] bytes)
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
                    packet.Left = new Hand();
                    packet.Right = new Hand();

                    short[] adcData = new short[ADC_COUNT * CYCLES];
                    System.Buffer.BlockCopy(packet.Buffer.Data, 0, adcData, 0, DATA_SIZE);
                    short[][] adc = new short[ADC_COUNT][];
                    for (int i = 0; i < ADC_COUNT; i++)
                    {
                        adc[i] = adcData.Where((s, j) => j >= CYCLES * i && j < CYCLES * (i + 1)).ToArray();
                    }

                    packet.Right.Const.AddRange(adc[0]);
                    packet.Right.Tremor.AddRange(adc[1]);
                    packet.Left.Const.AddRange(adc[2]);
                    packet.Left.Tremor.AddRange(adc[3]);
                }
                else if (packet.Buffer.Command == Commands.FromDevice.ADCCHECK) { }
            }
            return packet;
        }

        public static byte GetCommandFromBytes(byte[] buffer)
        {
            Debug.Assert(buffer != null && buffer.Length != 0);
            return buffer[0];
        }

        public byte GetControlSum()
        {
            byte sum = 0x00;
            for (int i = 0; i < DATA_SIZE; i++)
            {
                sum ^= Buffer.Data[i];
            }
            return sum;
        }

        public static byte[] Packet2Bytes(Packet packet)
        {
            byte[] buffer = new byte[Marshal.SizeOf(packet.Buffer)];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IEnumerable<short> adc = packet.Right.Const.AsEnumerable();
                adc = adc.Concat(packet.Right.Tremor);
                adc = adc.Concat(packet.Left.Const);
                adc = adc.Concat(packet.Left.Tremor);
                IList<short> data = adc as IList<short> ?? adc.ToList();
                if (data.Count * sizeof(short) == DATA_SIZE)
                {
                    System.Buffer.BlockCopy(data.ToArray(), 0, packet.Buffer.Data, 0, DATA_SIZE);
                }
                packet.ControlSum = packet.GetControlSum();

                System.IntPtr pBuffer = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(packet.Buffer, pBuffer, false);
            }
            finally
            {
                handle.Free();
            }
            return buffer;
        }

        private bool CheckValid()
        {
            byte controlSum = GetControlSum();
            if (controlSum != ControlSum)
                return false;
            return true;
        }

        // todo проверить Size
        // все работает, хотя не должно =/

        [StructLayout(LayoutKind.Sequential, Size = HEADER_SIZE, Pack = 1)]
        public struct PacketStruct
        {
            public byte Command;
            public ushort PackID;
            public ushort RequestID;
            public byte NU1;
            public byte NU2;
            public byte ControlSum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DATA_SIZE)]
            public byte[] Data;
        }
    }
}