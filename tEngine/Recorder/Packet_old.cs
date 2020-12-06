using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.Recorder {
    public class Packet_old : BPacket {
        public Hand Left { get; set; }
        public Hand Right { get; set; }

        public Packet_old() {
            Left = new Hand();
            Right = new Hand();
        }

        public Packet_old( Packet_old packetOld ) : base( packetOld ) {
            Debug.Assert( packetOld != null );
            Left = packetOld.Left;
            Right = packetOld.Right;
        }

        public static Packet_old BytesToPacket( byte[] bytes ) {
            var buffer = new DevicePack( 0, 0, 0 );
            var packet = new Packet_old();
            unsafe {
                Marshal.Copy( bytes, 0, (IntPtr) buffer.bytes, Packet_old.PACKET_SIZE );

                packet.PackId = buffer.Head.PackID;
                packet.RequestId = buffer.Head.RequestID;
                packet.Command = buffer.Head.Command;
                packet.ControlSum = buffer.Head.ControlSum;

                var adc1 = new short[Packet_old.VALUES_FROM_ADC];
                var adc2 = new short[Packet_old.VALUES_FROM_ADC];
                var adc3 = new short[Packet_old.VALUES_FROM_ADC];
                var adc4 = new short[Packet_old.VALUES_FROM_ADC];

                // левая
                Marshal.Copy( (IntPtr) buffer.adc1.data, adc1, 0, Packet_old.VALUES_FROM_ADC );
                Marshal.Copy( (IntPtr) buffer.adc2.data, adc2, 0, Packet_old.VALUES_FROM_ADC );
                // правая
                Marshal.Copy( (IntPtr) buffer.adc3.data, adc3, 0, Packet_old.VALUES_FROM_ADC );
                Marshal.Copy( (IntPtr) buffer.adc4.data, adc4, 0, Packet_old.VALUES_FROM_ADC );

                var left = new Hand();
                var right = new Hand();
                right.Const.AddRange( adc1 );
                right.Tremor.AddRange( adc2 );
                left.Const.AddRange( adc3 );
                left.Tremor.AddRange( adc4 );

                packet.SetHands( left, right );
            }
            return packet;
        }

        public Packet_old Clone() {
            var result = (Packet_old) Cloner.Clone( this );
            return result;
        }

        /// <summary>
        /// Контрольная сумма по измеренной информации
        /// </summary>
        /// <returns></returns>
        public byte GetControlSum() {
            var bytes = new List<UShort>();
            bytes.AddRange( Left.Const.ConvertAll( input => new UShort( input ) ) );
            bytes.AddRange( Left.Tremor.ConvertAll( input => new UShort( input ) ) );
            bytes.AddRange( Right.Const.ConvertAll( input => new UShort( input ) ) );
            bytes.AddRange( Right.Tremor.ConvertAll( input => new UShort( input ) ) );

            var sum = (byte) 0x00;
            bytes.ForEach( s => {
                unsafe {
                    sum ^= s.b[0];
                    sum ^= s.b[1];
                }
            } );

            return sum;
        }

        public static byte[] PacketToBytes( Packet_old packetOld ) {
            var buffer = new DevicePack( packetOld.Command, packetOld.RequestId, packetOld.PackId );
            buffer.Head.ControlSum = packetOld.ControlSum;
            var result = new byte[Packet_old.PACKET_SIZE];
            unsafe {
                var adc1 = new byte[Packet_old.BYTES_FROM_ADC];
                var adc2 = new byte[Packet_old.BYTES_FROM_ADC];
                var adc3 = new byte[Packet_old.BYTES_FROM_ADC];
                var adc4 = new byte[Packet_old.BYTES_FROM_ADC];
                CopyArrayPart( ShortToBytes( packetOld.Right.Const.ToArray() ), adc1 );
                CopyArrayPart( ShortToBytes( packetOld.Right.Tremor.ToArray() ), adc2 );
                CopyArrayPart( ShortToBytes( packetOld.Left.Const.ToArray() ), adc3 );
                CopyArrayPart( ShortToBytes( packetOld.Left.Tremor.ToArray() ), adc4 );

                Marshal.Copy( adc1, 0, (IntPtr) buffer.adc1.data, Packet_old.BYTES_FROM_ADC );
                Marshal.Copy( adc2, 0, (IntPtr) buffer.adc2.data, Packet_old.BYTES_FROM_ADC );
                Marshal.Copy( adc3, 0, (IntPtr) buffer.adc3.data, Packet_old.BYTES_FROM_ADC );
                Marshal.Copy( adc4, 0, (IntPtr) buffer.adc4.data, Packet_old.BYTES_FROM_ADC );

                Marshal.Copy( (IntPtr) buffer.bytes, result, 0, Packet_old.PACKET_SIZE );
            }
            return result;
        }

        public void SetHands( Hand left, Hand right ) {
            this.Left.Clear();
            this.Right.Clear();

            this.Left += left;
            this.Right += right;

            IsValid = CheckValid( this );
        }

        private bool CheckValid( Packet_old packetOld ) {
            var count = packetOld.Left.Const.Count;
            if( count != packetOld.Left.Tremor.Count ||
                count != packetOld.Right.Const.Count ||
                count != packetOld.Right.Tremor.Count )
                return false;

            byte controlSum = packetOld.GetControlSum();

            if( controlSum != packetOld.ControlSum )
                return false;
            return true;
        }

        [StructLayout( LayoutKind.Explicit, Size = BPacket.BYTES_FROM_ADC )]
        private unsafe struct ADC {
            [FieldOffset( 0 )]
            public fixed short data [BPacket.BYTES_FROM_ADC];

            [FieldOffset( 0 )]
            public short data1; // 2

            [FieldOffset( 2 )]
            public short data2; // 2

            [FieldOffset( 4 )]
            public short data3; // 2

            [FieldOffset( 6 )]
            public short data4; // 2

            [FieldOffset( 8 )]
            public short data5; // 2

            [FieldOffset( 10 )]
            public short data6; // 2

            [FieldOffset( 12 )]
            public short data7; // 2
        }

        [StructLayout( LayoutKind.Explicit, Size = BPacket.PACKET_SIZE )]
        private unsafe struct DevicePack {
            [FieldOffset( 0 )]
            public fixed byte bytes [BPacket.PACKET_SIZE];

            [FieldOffset( 0 )]
            public PackHead Head; // 8

            [FieldOffset( 8 )]
            public ADC adc1; // 14

            [FieldOffset( 22 )]
            public ADC adc2; // 14

            [FieldOffset( 36 )]
            public ADC adc3; // 14

            [FieldOffset( 50 )]
            public ADC adc4; // 14

            public DevicePack( byte command, ushort requestId, ushort packId ) {
                Head.Command = command;
                Head.RequestID = requestId;
                Head.PackID = packId;
                Head.Size = BPacket.PACKET_SIZE;
                Head.AdcSize = BPacket.BYTES_FROM_ADC;
                Head.ControlSum = 0;
                this.adc1 = new ADC();
                this.adc2 = new ADC();
                this.adc3 = new ADC();
                this.adc4 = new ADC();
            }
        }
    }
}