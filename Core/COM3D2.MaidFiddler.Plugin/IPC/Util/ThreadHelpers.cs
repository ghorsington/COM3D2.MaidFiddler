using System;
using System.Runtime.InteropServices;

namespace COM3D2.MaidFiddler.Core.IPC.Util
{
    public static class ThreadHelpers
    {
        private const int GENERIC_WRITE = 0x40000000;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentThreadId();

        public static bool CancelSynchronousIo(uint threadId)
        {
            if (threadId == 0)
                return false;
            IntPtr threadHandle = OpenThread(GENERIC_WRITE, false, threadId);
            bool ret = CancelSynchronousIo(threadHandle);
            CloseHandle(threadHandle);
            return ret;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenThread(uint desiredAccess, bool inheritHandle, uint threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CancelSynchronousIo(IntPtr threadHandle);
    }
}