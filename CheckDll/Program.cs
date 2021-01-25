using System;
using System.Threading;
using tEngine.Recorder;

namespace CheckDll
{
    class Program {
        static void Main( string[] args ) {
            var d = Device.CreateDevice(123);
            do {
                Console.Clear();

                if ( d.IsDllLoad() == false ) {
                    Console.WriteLine("Не удается загрузить DLL");
                }
                else {
                    var name = d.GetDllName();
                    Console.WriteLine("Загружена DLL - \"" + name + "\"");
                    Console.WriteLine( "Состояние устройства: " + d.DeviceState );
                }
                Thread.Sleep(200 );
            } while( true );
        }
    }
}
