using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tEngine.DataModel;
using tEngine.Recorder;

namespace tEngine.Test {
    [TestClass]
    public class SpeedTests {
        [TestMethod]
        public void DecodeBytes() {
            Console.WriteLine( "DecodeBytes:" );

            var volume = 10000.0;
            var device = Device.GetDevice( 0 );
            device.DemoMode = false;
            device.Stop();
            var bytes = device.GetBytes();
            var start = DateTime.Now;
            for( int i = 0; i < volume; i++ ) {
                var pack = Packet.Bytes2Packet( bytes );
            }
            var time = (DateTime.Now - start).TotalMilliseconds/volume;
            Console.WriteLine( "Packet2.Bytes2Packet - {0:F3} ms", time );

            Device.AbortAll();
        }

        [TestMethod]
        public void GetData() {
            Console.WriteLine( "GetData:" );

            var volume = 10000.0;
            var left = new Hand();
            var right = new Hand();

            var packetHere = 0;
            var device = Device.GetDevice( 0 );
            device.DemoMode = false;
            device.AddListener( ( id, hand1, hand2 ) => {
                left += hand1;
                right += hand2;
                packetHere++;
            });
            device.Start();
            //Thread.Sleep( 10000 );
            device.Stop();
            Console.WriteLine( "\tPack here -\t{0}", packetHere );
            Console.WriteLine( "\tInvalidPack -\t{0}", device.Counters.InvalidPack );
            Console.WriteLine( "\tValidPack -\t{0}", device.Counters.FullPack );
            Console.WriteLine( "\tLostPack -\t{0}", device.Counters.LostPack );
            Console.WriteLine( "\tRepeatPack -\t{0}", device.Counters.RepeatPack );
            Console.WriteLine( "\tTotalPack -\t{0}", device.Counters.TotalPack );
            Console.WriteLine( "\tPPS -\t\t{0:F3}", device.Counters.PPS.GetPs() );
            Console.WriteLine( "\tSPS -\t\t{0:F3}", device.Counters.ValidPPS.GetPs() * 28 );
            Console.WriteLine( "\tValidPPS:\t{0:F3}", device.Counters.ValidPPS.GetPs() );

            Device.AbortAll();
        }
    }
}