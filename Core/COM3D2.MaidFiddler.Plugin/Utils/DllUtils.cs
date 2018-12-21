using System;
using System.Runtime.InteropServices;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public static class DllUtils
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        public static T GetProcDelegate<T>(IntPtr hModule, string procedureName) where T : class
        {
            return Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, procedureName), typeof(T)) as T;
        }
    }
}
