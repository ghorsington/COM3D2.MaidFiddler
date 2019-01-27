using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MessagePack;

namespace COM3D2.MaidFiddler.Common.IPC
{
    public class ServiceHandler<T>
    {
        private static readonly MethodInfo deserialize =
                typeof(MessagePackSerializer).GetMethod(nameof(MessagePackSerializer.Deserialize), new[] {typeof(Stream), typeof(bool)});

        private static readonly MethodInfo serialize = typeof(MessagePackSerializer)
                                                       .GetMethods().First(m => m.Name == nameof(MessagePackSerializer.Serialize)
                                                                                && m.GetParameters().Length == 1
                                                                                && m.ReturnType == typeof(byte[]));

        private readonly Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
        private readonly T service;

        public ServiceHandler(T service)
        {
            this.service = service;
            InitService();
        }

        public IntPtr HandleMessage(string message, IntPtr data, long len, out long resLen)
        {
            if (!commands.TryGetValue(message, out var method))
                return Error($"Method {message} is not defined!", null, out resLen);

            var parameters = method.GetParameters();

            var invokeParams = new List<object>();

            if (data != IntPtr.Zero)
                try
                {
                    unsafe
                    {
                        using (var stream = new UnmanagedMemoryStream((byte*) data.ToPointer(), len))
                            invokeParams.AddRange(parameters.Select(param => deserialize.MakeGenericMethod(param.ParameterType))
                                                            .Select(deserializer =>
                                                                            deserializer.Invoke(null, new object[] {stream, true})));
                    }
                }
                catch (Exception e)
                {
                    return Error($"Failed to deserialize arguments: {message}", e.StackTrace, out resLen);
                }

            object result;
            try
            {
                result = method.Invoke(service, invokeParams.ToArray());
            }
            catch (Exception e)
            {
                return Error(e.InnerException?.Message ?? e.Message, e.InnerException?.StackTrace ?? e.StackTrace, out resLen);
            }

            if (method.ReturnType == typeof(void))
            {
                resLen = 0;
                return IntPtr.Zero;
            }

            byte[] serialized;
            try
            {
                serialized = (byte[]) serialize.MakeGenericMethod(method.ReturnType).Invoke(null, new[] {result});
            }
            catch (Exception e)
            {
                return Error($"Failed to serialize the result: {e.InnerException?.Message ?? e.Message}",
                             e.InnerException?.StackTrace ?? e.StackTrace, out resLen);
            }

            var ptr = Marshal.AllocHGlobal(serialized.Length);
            Marshal.Copy(serialized, 0, ptr, serialized.Length);
            resLen = serialized.Length;
            return ptr;
        }

        private void InitService()
        {
            foreach (var methodInfo in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                commands[methodInfo.Name] = methodInfo;
        }

        private IntPtr Error(string message, string stackTrace, out long resLen)
        {
            using (var ms = new MemoryStream())
            {
                // Write a flag byte to distinguish real results from errors
                ms.WriteByte(0x00);
                MessagePackSerializer.Serialize(ms, new Error {Message = message, StackTrace = stackTrace});
                resLen = ms.Length;
                var result = Marshal.AllocHGlobal((int)ms.Length);
                Marshal.Copy(ms.GetBuffer(), 0, result, (int)ms.Length);
                return result;
            }
        }
    }
}