using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using tEngine.Helpers;

namespace tEngine.Recorder {
    public class Collector {
#if DEBUG
        private const string DllPostfix = "_d";
#else        
        private const string DllPostfix = string.Empty;
#endif
        public string DllName = "none";
        private IntPtr mDLLFile = IntPtr.Zero;
        private DLLIsConnect mDLLIsConnect = null;
        private USBInit mUSBInit = null;
        private USBIsConnect mUSBIsConnect = null;
        private USBReadData mUSBReadData = null;
        private USBWriteData mUSBWriteData = null;

        public Collector() {
            var names = new List<string>() {
                $@"TenzoDevice{DllPostfix}.dll",
                $@"lib\TenzoDevice{DllPostfix}.dll",
            };
            var dinfo = new DirectoryInfo( Directory.GetCurrentDirectory() );
            var files_dop = dinfo.GetFiles( "TenzoDevice*.dll" );
            names.AddRange( files_dop.Select( info => info.FullName ) );
            foreach( var name in names ) {
                mDLLFile = NativeMethods.LoadLibrary( name );
                if( mDLLFile == IntPtr.Zero ) continue;

                var addr = NativeMethods.GetProcAddress( mDLLFile, "DLLIsConnect" );
                if( addr == IntPtr.Zero ) continue;

                var check = (DLLIsConnect) Marshal.GetDelegateForFunctionPointer( addr, typeof( DLLIsConnect ) );

                if( check() == true ) {
                    DllName = name;
                    InitMethods( mDLLFile );
                    return;
                } else {
                    NativeMethods.FreeLibrary( mDLLFile );
                }
            }
        }

        public bool InitUsb() {
            try {
                if( mUSBInit != null )
                    return mUSBInit();
                return false;
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
                return false;
            }
        }

        public bool IsDeviceConnect() {
            try {
                if( mUSBIsConnect != null )
                    return mUSBIsConnect();
                return false;
            } catch( Exception ex ) {
                // dll не загрузилась
                Debug.Assert( false, ex.Message );
                return false;
            }
        }

        public bool IsDllConnect() {
            try {
                if( mDLLIsConnect != null )
                    return mDLLIsConnect();
                return false;
            } catch( Exception ex ) {
                // dll не загрузилась
                Debug.Assert( false, ex.Message );
                return false;
            }
        }

        public bool IsDllLoad() {
            return mDLLFile != IntPtr.Zero;
        }

        public bool ReadData( ref byte[] buffer ) {
            try {
                if( mUSBReadData == null ) return false;
                var pBuf = Marshal.AllocHGlobal( 64 );
                var result = mUSBReadData( pBuf, 64 );
                Marshal.Copy( pBuf, buffer, 0, 64 );
                return result;
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
                return false;
            }
        }

        public void UnLoad() {
            if( IsDllLoad() )
                NativeMethods.FreeLibrary( mDLLFile );
        }

        public bool WriteData( byte[] buffer ) {
            try {
                if( mUSBWriteData == null ) return false;
                var pBuf = Marshal.AllocHGlobal( 64 );
                Marshal.Copy( buffer, 0, pBuf, 64 );
                var result = mUSBWriteData( pBuf, 64 );
                return result;
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
                return false;
            }
        }

        private void InitMethods( IntPtr dll ) {
            try {
                var addr = NativeMethods.GetProcAddress( dll, "DLLIsConnect" );
                mDLLIsConnect = (DLLIsConnect) Marshal.GetDelegateForFunctionPointer( addr, typeof( DLLIsConnect ) );

                addr = NativeMethods.GetProcAddress( dll, "USBInit" );
                mUSBInit = (USBInit) Marshal.GetDelegateForFunctionPointer( addr, typeof( USBInit ) );

                addr = NativeMethods.GetProcAddress( dll, "USBIsConnect" );
                mUSBIsConnect = (USBIsConnect) Marshal.GetDelegateForFunctionPointer( addr, typeof( USBIsConnect ) );

                addr = NativeMethods.GetProcAddress( dll, "USBReadData" );
                mUSBReadData = (USBReadData) Marshal.GetDelegateForFunctionPointer( addr, typeof( USBReadData ) );

                addr = NativeMethods.GetProcAddress( dll, "USBWriteData" );
                mUSBWriteData = (USBWriteData) Marshal.GetDelegateForFunctionPointer( addr, typeof( USBWriteData ) );
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
            }
        }

        private static class NativeMethods {
            [DllImport( "kernel32.dll" )]
            public static extern bool FreeLibrary( IntPtr hModule );

            [DllImport( "kernel32.dll" )]
            public static extern IntPtr GetProcAddress( IntPtr hModule, string procedureName );

            [DllImport( "kernel32.dll" )]
            public static extern IntPtr LoadLibrary( string dllToLoad );
        }

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate bool DLLIsConnect();

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate bool USBIsConnect();

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate bool USBInit();

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate bool USBReadData( IntPtr buffer, int size );

        [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
        private delegate bool USBWriteData( IntPtr buffer, int size );

        #region FakeData

        private static int FakeFirst = 0;
        private static ushort FakePackID = 0;
        private static ushort FakeRequestID = 0;

        public static ushort FakeRequestId {
            set { FakeRequestID = value; }
        }

        private static short FakeSin( int param ) {
            var result = Convert.ToInt16( (Math.Sin( 0.02*param*Math.PI/180.0 ))*20000.0 );
            return Math.Abs( result );
            //return result;
        }

        public static bool ReadFakeData( ref byte[] buffer ) {
            try {
                var adc = Enumerable.Range( FakeFirst, Packet.CYCLES ).Select( FakeSin ).ToList();
                FakeFirst += Packet.CYCLES;
                var pack = new Packet();
                pack.PackId = FakePackID++;
                pack.RequestId = FakeRequestID;
                pack.Command = Commands.FromDevice.DATA;

                pack.Left.Const.AddRange( adc );
                pack.Left.Tremor.AddRange( adc.Select( s => (short) (s/10.0) ) );
                pack.Right.Const.AddRange( adc.Select( s => (short) (s + 2000) ) );
                pack.Right.Tremor.AddRange( adc.Select( s => (short) ((s + 2000)/10.0) ) );

                 buffer = Packet.Packet2Bytes( pack );

                Thread.Sleep( 2 );
                return true;
            } catch( Exception ex ) {
                return false;
            }
        }

        #endregion FakeData

        #region Function From *DLL_NAME*

        //[DllImport( DLL_NAME, CallingConvention = CallingConvention.Cdecl )]
        //private static extern bool DLLIsConnect();

        //[DllImport( DLL_NAME, CallingConvention = CallingConvention.Cdecl )]
        //private static extern bool USBInit();

        //[DllImport( DLL_NAME, CallingConvention = CallingConvention.Cdecl )]
        //private static extern bool USBIsConnect();

        //[DllImport( DLL_NAME, CallingConvention = CallingConvention.Cdecl )]
        //private static extern bool USBReadData( IntPtr buffer, int size );

        //[DllImport( DLL_NAME, CallingConvention = CallingConvention.Cdecl )]
        //private static extern bool USBWriteData( IntPtr buffer, int size );

        #endregion Function From *DLL_NAME*
    }
}