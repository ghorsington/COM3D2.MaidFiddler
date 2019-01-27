using System;

namespace COM3D2.MaidFiddler.Common.IPC
{
    public delegate IntPtr HandleMessageDelegate(string message, IntPtr data, long len, out long outLen);

    public delegate byte[] SendMessageDelegate(string methodName, byte[] data);
}
