using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tEngine.Recorder;

namespace tEngine.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void AdcCheckMode() {
            var d = Device.CreateDevice( 11 );
            d.AdcTestCallBack = AdcTestCallBack;
            d.DemoMode = false;
            while( d.SetMode( Device.WorkMode.AdcCheck ) == false ) {
                Thread.Sleep( 30 );
            }
            do {
                Thread.Sleep( 30 );
            } while( true );
        }

        [TestMethod]
        public void DllLoad() {
            var d = Device.CreateDevice( 11 );
            do {
                Thread.Sleep( 30 );
            } while( true );
        }

        [TestMethod]
        public void TestStruct() {
            var dp = new DevicePack();
        }

        private void AdcTestCallBack( bool[] dr, short[] adc ) {
            if( dr == null || adc == null ) return;
            if( dr.Length == 4 && adc.Length == 4 ) {
                Console.Clear();
                Console.WriteLine( dr );
                Console.WriteLine( adc );
            }
        }

        [StructLayout( LayoutKind.Explicit, Size = 10 )]
        private unsafe struct DevicePack {
            [FieldOffset( 0 )] public fixed byte bytes [10];

            [FieldOffset( 0 )] public byte DR1; // 4
            [FieldOffset( 2 )] public byte DR2; // 4
            [FieldOffset( 3 )] public byte DR3; // 4
            [FieldOffset( 4 )] public byte DR4; // 4
        }
    }
}