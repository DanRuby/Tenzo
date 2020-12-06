using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace tEngine.Recorder {
    internal class TestPacket_old : BPacket {
        public short[] AdcData = new short[4];
        public bool[] DataReady = new bool[4];

        public TestPacket_old() {
            base.IsValid = true;
        }

        public static TestPacket_old BytesToPacket( byte[] bytes ) {
            var buffer = new DevicePack( 0, 0, 0 );
            var packet = new TestPacket_old();
            unsafe {
                Marshal.Copy( bytes, 0, (IntPtr) buffer.bytes, PACKET_SIZE );

                packet.PackId = buffer.Head.PackID;
                packet.RequestId = buffer.Head.RequestID;
                packet.Command = buffer.Head.Command;
                packet.ControlSum = buffer.Head.ControlSum;
                var dr = new[] {buffer.DR1, buffer.DR2, buffer.DR3, buffer.DR4};
                var adc = new[] {buffer.Adc1, buffer.Adc2, buffer.Adc3, buffer.Adc4};

                for( int i = 0; i < 4; i++ ) {
                    packet.DataReady[i] = dr[i] != 0x00;
                    packet.AdcData[i] = adc[i];
                }
            }
            return packet;
        }

        [StructLayout( LayoutKind.Explicit, Size = BPacket.PACKET_SIZE )]
        private unsafe struct DevicePack {
            [FieldOffset( 0 )]
            public fixed byte bytes [BPacket.PACKET_SIZE];

            [FieldOffset( 0 )]
            public PackHead Head; // 8

            [FieldOffset( 8 )]
            public byte DR1; // 1

            [FieldOffset( 9 )]
            public byte DR2; // 1

            [FieldOffset( 10 )]
            public byte DR3; // 1

            [FieldOffset( 11 )]
            public byte DR4; // 1

            [FieldOffset( 12 )]
            public short Adc1; // 2

            [FieldOffset( 14 )]
            public short Adc2; // 2

            [FieldOffset( 16 )]
            public short Adc3; // 2

            [FieldOffset( 18 )]
            public short Adc4; // 2


            public DevicePack( byte command, ushort requestId, ushort packId ) {
                Head.Command = command;
                Head.RequestID = requestId;
                Head.PackID = packId;
                Head.Size = BPacket.PACKET_SIZE;
                Head.AdcSize = BPacket.BYTES_FROM_ADC;
                Head.ControlSum = 0;
                DR1 = 0;
                DR2 = 0;
                DR3 = 0;
                DR4 = 0;
                Adc1 = 0;
                Adc2 = 0;
                Adc3 = 0;
                Adc4 = 0;
            }
        }
    }
}