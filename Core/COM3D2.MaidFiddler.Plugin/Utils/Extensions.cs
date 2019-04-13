using System;
using System.Collections.Generic;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public static class Extensions
    {
        private static readonly HashSet<Type> NativeFileTypes = new HashSet<Type>
        {
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool)
        };

        public static bool IsNativeType(this Type self)
        {
            return NativeFileTypes.Contains(self) || NativeFileTypes.Contains(Nullable.GetUnderlyingType(self));
        }
    }
}