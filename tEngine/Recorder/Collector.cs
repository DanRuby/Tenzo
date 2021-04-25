using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace tEngine.Recorder
{
    /// <summary>
    /// Д: загружет длл и работает с устройством по юсб
    /// </summary>
    public class Collector
    {
#if DEBUG
        private const string DllPostfix = "_d";
#else        
        private const string DllPostfix = "";
#endif
        public string DllName = "none";
        private IntPtr mDLLFile = IntPtr.Zero;
        private DLLIsConnect mDLLIsConnect = null;
        private USBInit mUSBInit = null;
        private USBIsConnect mUSBIsConnect = null;
        private USBReadData mUSBReadData = null;
        private USBWriteData mUSBWriteData = null;

        public Collector()
        {
            List<string> names = new List<string>() {
                $@"TenzoDevice{DllPostfix}.dll",
                $@"lib\TenzoDevice{DllPostfix}.dll",
            };
            DirectoryInfo dinfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files_dop = dinfo.GetFiles("TenzoDevice*.dll");
            names.AddRange(files_dop.Select(info => info.FullName));
            foreach (string name in names)
            {
                mDLLFile = NativeMethods.LoadLibrary(name);
                if (mDLLFile == IntPtr.Zero)
                    continue;

                IntPtr addr = NativeMethods.GetProcAddress(mDLLFile, "DLLIsConnect");
                if (addr == IntPtr.Zero)
                    continue;

                DLLIsConnect check = (DLLIsConnect)Marshal.GetDelegateForFunctionPointer(addr, typeof(DLLIsConnect));

                if (check() == true)
                {
                    DllName = name;
                    InitMethods(mDLLFile);
                    return;
                }
                else
                {
                    NativeMethods.FreeLibrary(mDLLFile);
                }
            }
        }

        public bool InitUsb()
        {
            try
            {
                if (mUSBInit != null)
                    return mUSBInit();
                return false;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                return false;
            }
        }

        public bool IsDeviceConnect()
        {
            try
            {
                if (mUSBIsConnect != null)
                    return mUSBIsConnect();
                return false;
            }
            catch (Exception ex)
            {
                // dll не загрузилась
                Debug.Assert(false, ex.Message);
                return false;
            }
        }

        public bool IsDllConnect()
        {
            try
            {
                if (mDLLIsConnect != null)
                    return mDLLIsConnect();
                return false;
            }
            catch (Exception ex)
            {
                // dll не загрузилась
                Debug.Assert(false, ex.Message);
                return false;
            }
        }

        public bool IsDllLoad() => mDLLFile != IntPtr.Zero;

        public bool ReadData(ref byte[] buffer)
        {
            try
            {
                if (mUSBReadData == null)
                    return false;
                IntPtr pBuf = Marshal.AllocHGlobal(64);
                bool result = mUSBReadData(pBuf, 64);
                Marshal.Copy(pBuf, buffer, 0, 64);
                return result;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                return false;
            }
        }

        public void UnLoad()
        {
            if (IsDllLoad())
                NativeMethods.FreeLibrary(mDLLFile);
        }

        public bool WriteData(byte[] buffer)
        {
            try
            {
                if (mUSBWriteData == null) return false;
                IntPtr pBuf = Marshal.AllocHGlobal(64);
                Marshal.Copy(buffer, 0, pBuf, 64);
                bool result = mUSBWriteData(pBuf, 64);
                return result;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Д: присваивает остальным делегатам адресы из загруженной длл
        /// </summary>
        /// <param name="dll"></param>
        private void InitMethods(IntPtr dll)
        {
            try
            {
                IntPtr addr = NativeMethods.GetProcAddress(dll, "DLLIsConnect");
                mDLLIsConnect = (DLLIsConnect)Marshal.GetDelegateForFunctionPointer(addr, typeof(DLLIsConnect));

                addr = NativeMethods.GetProcAddress(dll, "USBInit");
                mUSBInit = (USBInit)Marshal.GetDelegateForFunctionPointer(addr, typeof(USBInit));

                addr = NativeMethods.GetProcAddress(dll, "USBIsConnect");
                mUSBIsConnect = (USBIsConnect)Marshal.GetDelegateForFunctionPointer(addr, typeof(USBIsConnect));

                addr = NativeMethods.GetProcAddress(dll, "USBReadData");
                mUSBReadData = (USBReadData)Marshal.GetDelegateForFunctionPointer(addr, typeof(USBReadData));

                addr = NativeMethods.GetProcAddress(dll, "USBWriteData");
                mUSBWriteData = (USBWriteData)Marshal.GetDelegateForFunctionPointer(addr, typeof(USBWriteData));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }

        /// <summary>
        /// Д: методы ядра для длл и нахождения адреса функции 
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DLLIsConnect();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool USBIsConnect();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool USBInit();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool USBReadData(IntPtr buffer, int size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool USBWriteData(IntPtr buffer, int size);

        /// <summary>
        /// Д: как понимаю симуляция синуса для демоверсии
        /// </summary>

        #region TestData

        private static int FakeFirst = 0;
        private static ushort FakePackID = 0;
        private static ushort FakeRequestID = 0;

        public static ushort FakeRequestId
        {
            set => FakeRequestID = value;
        }

        private static short FakeSin(int param)
        {
            short result = Convert.ToInt16((Math.Sin(0.02 * param * Math.PI / 180.0)) * 20000.0);
            return Math.Abs(result);
        }

        public static bool ReadFakeData(ref byte[] buffer)
        {
            try
            {
                List<short> adc = Enumerable.Range(FakeFirst, Packet.CYCLES).Select(FakeSin).ToList();
                FakeFirst += Packet.CYCLES;
                Packet pack = new Packet();
                pack.PackId = FakePackID++;
                pack.RequestId = FakeRequestID;
                pack.Command = Commands.FromDevice.DATA;

                pack.Left.Const.AddRange(adc);
                pack.Left.Tremor.AddRange(adc.Select(s => (short)(s / 10.0)));
                pack.Right.Const.AddRange(adc.Select(s => (short)(s + 2000)));
                pack.Right.Tremor.AddRange(adc.Select(s => (short)((s + 2000) / 10.0)));

                buffer = Packet.Packet2Bytes(pack);

                Thread.Sleep(2);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion TestData

    }
}