using System;
using System.Collections.Generic;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public static class Extensions
    {
        private static readonly HashSet<Type> FloatTypes = new HashSet<Type> {typeof(float), typeof(double), typeof(decimal)};

        private static readonly HashSet<Type> IntegerTypes = new HashSet<Type>
        {
                typeof(sbyte),
                typeof(short),
                typeof(int),
                typeof(long),
        };

        private static readonly HashSet<Type> UnsignedIntegerTypes = new HashSet<Type>
        {
                typeof(byte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong)
        };

        public static bool IsSignedInteger(this Type self)
        {
            return IntegerTypes.Contains(self) || IntegerTypes.Contains(Nullable.GetUnderlyingType(self));
        }

        public static bool IsUnsignedInteger(this Type self)
        {
            return UnsignedIntegerTypes.Contains(self) || UnsignedIntegerTypes.Contains(Nullable.GetUnderlyingType(self));
        }

        public static bool IsFloat(this Type self)
        {
            return FloatTypes.Contains(self) || FloatTypes.Contains(Nullable.GetUnderlyingType(self));
        }
    }
}