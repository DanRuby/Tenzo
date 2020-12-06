using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace tEngine.Recorder {
    public class BPacket {
        public const int BYTES_FROM_ADC = 14;
        public const int PACKET_HEAD_SIZE = 8;
        public const int PACKET_SIZE = 64;
        public const int VALUES_FROM_ADC = BYTES_FROM_ADC/sizeof( short );
        public byte Command { get; set; }
        public byte ControlSum { get; set; }
        public bool IsValid { get; set; }
        public ushort PackId { get; set; }
        public ushort RequestId { get; set; }

        public BPacket() {
            Command = 0x00;
            ControlSum = 0x00;
            RequestId = 0x00;
            PackId = 0x00;
            IsValid = false;
        }

        public BPacket( BPacket packetOld ) {
            Debug.Assert( packetOld != null );
            Command = packetOld.Command;
            ControlSum = packetOld.ControlSum;
            RequestId = packetOld.RequestId;
            PackId = packetOld.PackId;
            IsValid = packetOld.IsValid;
        }

        public static byte GetCommand( byte[] buffer ) {
            Debug.Assert( buffer != null && buffer.Length != 0 );
            return buffer[0];
        }

        protected static short[] BytesToShort( byte[] bytes ) {
            var shorts = new List<short>();
            for( int i = 0; i < bytes.Length; i += 2 ) {
                var value = 0x00;
                value += bytes[i + 1];
                value += bytes[i] << 16;
                shorts.Add( (short) value );
            }
            return shorts.ToArray();
        }

        protected static void CopyArrayPart( Array source, Array dest ) {
            var length = source.Length > dest.Length ? dest.Length : source.Length;
            Array.Copy( source, dest, length );
        }

        protected static byte[] ShortToBytes( short[] shorts ) {
            var list = shorts.ToList().ConvertAll( input => new UShort( input ) );
            var bytes = new List<byte>();
            list.ForEach( s => {
                unsafe {
                    bytes.Add( s.b[0] );
                    bytes.Add( s.b[1] );
                }
            } );
            return bytes.ToArray();
        }

        [StructLayout( LayoutKind.Explicit, Size = 2 )]
        protected unsafe struct UShort {
            [FieldOffset( 0 )]
            public fixed byte b [2];

            [FieldOffset( 0 )]
            public short s;

            public UShort( short shrt ) {
                this.s = shrt;
            }
        }

        [StructLayout( LayoutKind.Explicit, Size = BPacket.PACKET_HEAD_SIZE )]
        protected unsafe struct PackHead {
            [FieldOffset( 0 )]
            public byte Command; // 1

            [FieldOffset( 1 )]
            public ushort PackID; // 2

            [FieldOffset( 3 )]
            public ushort RequestID; // 2

            [FieldOffset( 5 )]
            public byte Size; // 1

            [FieldOffset( 6 )]
            public byte AdcSize; // 1

            [FieldOffset( 7 )]
            public byte ControlSum; // 1
        }
    }
}