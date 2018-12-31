using System;
using System.Collections.Generic;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public static class TypeExtensions
    {
        private static readonly HashSet<Type> FloatTypes = new HashSet<Type> {typeof(float), typeof(double), typeof(decimal)};

        private static readonly HashSet<Type> IntegerTypes = new HashSet<Type>
        {
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong)
        };

        public static bool IsInteger(this Type self)
        {
            return IntegerTypes.Contains(self) || IntegerTypes.Contains(Nullable.GetUnderlyingType(self));
        }

        public static bool IsFloat(this Type self)
        {
            return FloatTypes.Contains(self) || FloatTypes.Contains(Nullable.GetUnderlyingType(self));
        }
    }
}