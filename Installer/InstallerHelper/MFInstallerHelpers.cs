using System.Diagnostics;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace InstallerHelper
{
    public static class MFInstallerHelpers
    {
        [DllExport("IsProcessOpened", CallingConvention.StdCall)]
        public static bool IsProcessOpened([MarshalAs(UnmanagedType.LPWStr)] string friendlyName) => Process.GetProcessesByName(friendlyName).Length != 0;
    }
}