using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MessagePack;

namespace COM3D2.MaidFiddler.Common.IPC
{
    public static class ServiceProxyGenerator
    {
        private static readonly Dictionary<Type, Type> proxyImplementations = new Dictionary<Type, Type>();
        private static readonly ModuleBuilder module;
        private static readonly AssemblyBuilder assembly;

        private static readonly MethodInfo deserialize = typeof(MessagePackSerializer).GetMethod(nameof(MessagePackSerializer.Deserialize), new[] {typeof(byte[])});
        private static readonly ConstructorInfo memoryStreamCtor = typeof(MemoryStream).GetConstructor(Type.EmptyTypes);
        private static readonly MethodInfo memoryStreamDispose = typeof(MemoryStream).GetMethod(nameof(MemoryStream.Dispose));
        private static readonly MethodInfo memoryStreamGetBuffer = typeof(MemoryStream).GetMethod(nameof(MemoryStream.GetBuffer));
        private static readonly MethodInfo serialize = typeof(MessagePackSerializer).GetMethods().First(m =>
        {
            if (m.Name != nameof(MessagePackSerializer.Serialize))
                return false;
            var p = m.GetParameters();
            if (p.Length != 2)
                return false;
            if (p[0].ParameterType != typeof(Stream))
                return false;
            return true;
        });

        static ServiceProxyGenerator()
        {
            var name = new AssemblyName("__GeneratedProxies");
            assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            module = assembly.DefineDynamicModule(name.Name, $"{name.Name}.dll");
        }

        public static T GenerateServiceProxy<T>(SendMessageDelegate sendMessage)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("The given type must be an interface");

            if (proxyImplementations.TryGetValue(typeof(T), out var proxyImplementation))
                return (T) Activator.CreateInstance(proxyImplementation, sendMessage);

            var implType = module.DefineType($"{typeof(T).Name}_ProxyImpl", TypeAttributes.Public, null, new[] {typeof(T)});
            var invokeMethod = sendMessage.GetType().GetMethod("Invoke");

            var sendMessageDelegateField = implType.DefineField("sendMessage", typeof(SendMessageDelegate), FieldAttributes.Private);

            // Generate constructor that takes the delegate
            var ctor = implType.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] {typeof(SendMessageDelegate)});
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, sendMessageDelegateField);
            il.Emit(OpCodes.Ret);

            // Start implementing all the methods
            foreach (var methodInfo in typeof(T).GetMethods())
            {
                var parameters = methodInfo.GetParameters();
                var meth = implType.DefineMethod(methodInfo.Name,
                                                 MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Final
                                                 | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                                                 CallingConventions.HasThis,
                                                 methodInfo.ReturnType,
                                                 parameters.Select(p => p.ParameterType).ToArray());

                il = meth.GetILGenerator();
                LocalBuilder resultStore = null;

                if (methodInfo.ReturnType != typeof(void))
                    resultStore = il.DeclareLocal(methodInfo.ReturnType);

                if (parameters.Length != 0)
                {
                    GenerateParameterSerializer(methodInfo, il, sendMessageDelegateField, invokeMethod, resultStore);
                }
                else
                {
                    // sendMessage.Invoke(string, ms.GetBuffer())
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, sendMessageDelegateField);
                    il.Emit(OpCodes.Ldstr, methodInfo.Name);
                    il.Emit(OpCodes.Ldnull);
                    GenerateSendMessageInvoke(il, methodInfo, invokeMethod, resultStore);
                }

                if (methodInfo.ReturnType != typeof(void))
                    il.Emit(OpCodes.Ldloc, resultStore);

                il.Emit(OpCodes.Ret);
            }

            proxyImplementation = implType.CreateType();
            proxyImplementations[typeof(T)] = proxyImplementation;

            return (T) Activator.CreateInstance(proxyImplementation, sendMessage);
        }

        private static void GenerateParameterSerializer(MethodInfo target,
                                                        ILGenerator il,
                                                        FieldBuilder delegateField,
                                                        MethodInfo invokeMethod,
                                                        LocalBuilder resultStore)
        {
            var parameters = target.GetParameters();
            var loc = il.DeclareLocal(typeof(MemoryStream));

            // var ms = new MemoryStream()
            il.Emit(OpCodes.Newobj, memoryStreamCtor);
            il.Emit(OpCodes.Stloc, loc);

            il.BeginExceptionBlock(); // try {
            {
                for (int index = 0; index < parameters.Length; index++)
                {
                    var param = parameters[index].ParameterType;
                    var typedSerialize = serialize.MakeGenericMethod(param);

                    // MessagePackSerializer.Serialize(ms, arg_i)
                    il.Emit(OpCodes.Ldloc, loc);
                    il.Emit(OpCodes.Ldarg, index + 1);
                    il.Emit(OpCodes.Call, typedSerialize);
                }

                // sendMessage.Invoke(string, ms.GetBuffer())
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, delegateField);
                il.Emit(OpCodes.Ldstr, target.Name);
                il.Emit(OpCodes.Ldloc, loc);
                il.Emit(OpCodes.Call, memoryStreamGetBuffer);

                GenerateSendMessageInvoke(il, target, invokeMethod, resultStore);
            }
            il.BeginFinallyBlock(); // finally {
            {
                // ms.Dispose()
                il.Emit(OpCodes.Ldloc, loc);
                il.Emit(OpCodes.Call, memoryStreamDispose);
            }
            il.EndExceptionBlock(); // }
        }

        private static void GenerateSendMessageInvoke(ILGenerator il,
                                                      MethodInfo targetMethod,
                                                      MethodInfo invokeMethod,
                                                      LocalBuilder resultStore)
        {
            il.Emit(OpCodes.Callvirt, invokeMethod);

            if (targetMethod.ReturnType != typeof(void))
            {
                // MessagePackSerializer.Deserialize(result)
                il.Emit(OpCodes.Call, deserialize.MakeGenericMethod(targetMethod.ReturnType));
                il.Emit(OpCodes.Stloc, resultStore);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }
        }
    }
}