using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MessagePack;

namespace MiniIPC
{
    internal static class DynamicCodeGenerator
    {
        private static readonly AssemblyBuilder assembly;

        private static readonly MethodInfo deserialize =
                typeof(MessagePackSerializer).GetMethod(nameof(MessagePackSerializer.Deserialize), new[] {typeof(byte[])});

        private static readonly MethodInfo deserializeBytes =
                typeof(MessagePackSerializer).GetMethod(nameof(MessagePackSerializer.Deserialize), new[] {typeof(byte[]), typeof(int), typeof(IFormatterResolver), typeof(int).MakeByRefType()});

        private static readonly ConstructorInfo memoryStreamCtor = typeof(MemoryStream).GetConstructor(Type.EmptyTypes);
        private static readonly MethodInfo memoryStreamDispose = typeof(MemoryStream).GetMethod(nameof(MemoryStream.Dispose));

        private static readonly MethodInfo memoryStreamGetBuffer = typeof(MemoryStream).GetMethod(nameof(MemoryStream.GetBuffer));

        private static readonly ModuleBuilder module;

        private static readonly MethodInfo getDefaultResolver =
                typeof(MessagePackSerializer).GetProperty(nameof(MessagePackSerializer.DefaultResolver)).GetGetMethod();

        private static readonly MethodInfo serialize = typeof(MessagePackSerializer).GetMethods()
                                                                                    .First(m =>
                                                                                     {
                                                                                         if (m.Name != nameof(MessagePackSerializer
                                                                                                                     .Serialize))
                                                                                             return false;
                                                                                         var p = m.GetParameters();
                                                                                         if (p.Length != 2)
                                                                                             return false;
                                                                                         if (p[0].ParameterType != typeof(Stream))
                                                                                             return false;
                                                                                         return true;
                                                                                     });

        private static readonly MethodInfo serializeBytes =
                typeof(MessagePackSerializer).GetMethods().First(m => m.Name == nameof(MessagePackSerializer.Serialize) && m.GetParameters().Length == 1);

        static DynamicCodeGenerator()
        {
            var name = new AssemblyName("__GeneratedProxies");
            assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            module = assembly.DefineDynamicModule(name.Name, $"{name.Name}.dll");
        }

        public static Type MakeReceiverProxy<T>(out MethodInfo invokeMethod, out Dictionary<string, int> methodMap)
        {
            var implType = module.DefineType($"{typeof(T).Name}_ReceiveProxy", TypeAttributes.Public, null);

            // Object that receives the messages
            var serviceObject = implType.DefineField("serviceObject", typeof(T), FieldAttributes.Private);

            // Generate constructor that takes the service object
            var ctor = implType.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] {typeof(T)});
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, serviceObject);
            il.Emit(OpCodes.Ret);

            var proxyMethods = new List<MethodBuilder>();

            methodMap = new Dictionary<string, int>();

            // Implement unpacker and invoker methods
            foreach (var methodInfo in typeof(T).GetMethods())
            {
                var meth = implType.DefineMethod(methodInfo.Name,
                                                 MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Final
                                               | MethodAttributes.NewSlot,
                                                 CallingConventions.HasThis,
                                                 typeof(byte[]),
                                                 new[] {typeof(byte[]), typeof(int)});
                proxyMethods.Add(meth);

                bool isVoid = methodInfo.ReturnType == typeof(void);
                var parameters = methodInfo.GetParameters();

                il = meth.GetILGenerator();

                if (parameters.Length != 0)
                {
                    var readSize = il.DeclareLocal(typeof(int));
                    var defaultResolver = il.DeclareLocal(typeof(IFormatterResolver));

                    il.Emit(OpCodes.Call, getDefaultResolver);
                    il.Emit(OpCodes.Stloc, defaultResolver);

                    var args = new List<LocalBuilder>();

                    bool first = true;

                    foreach (var parameterInfo in parameters)
                    {
                        var arg = il.DeclareLocal(parameterInfo.ParameterType);
                        args.Add(arg);

                        if (!first)
                        {
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Ldloc, readSize);
                            il.Emit(OpCodes.Add);
                            il.Emit(OpCodes.Starg, 2);
                        }
                        first = false;

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Ldloc, defaultResolver);
                        il.Emit(OpCodes.Ldloca, readSize);
                        il.Emit(OpCodes.Call, deserializeBytes.MakeGenericMethod(parameterInfo.ParameterType));
                        il.Emit(OpCodes.Stloc, arg);
                    }

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, serviceObject);
                    foreach (var argLocal in args)
                        il.Emit(OpCodes.Ldloc, argLocal);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, serviceObject);
                }

                il.Emit(OpCodes.Callvirt, methodInfo);

                if (!isVoid)
                    il.Emit(OpCodes.Call, serializeBytes.MakeGenericMethod(methodInfo.ReturnType));
                else
                    il.Emit(OpCodes.Ldnull);

                il.Emit(OpCodes.Ret);
            }

            // Define the main invoker
            var invokeMeth = implType.DefineMethod($"{implType}__Invoke",
                                                   MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Final
                                                 | MethodAttributes.NewSlot,
                                                   CallingConventions.HasThis,
                                                   typeof(byte[]),
                                                   new[] {typeof(int), typeof(byte[]), typeof(int)});

            il = invokeMeth.GetILGenerator();

            var jmpLabels = proxyMethods.Select(s => il.DefineLabel()).ToArray();
            var defaultLabel = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Switch, jmpLabels);
            il.Emit(OpCodes.Br, defaultLabel);

            for (int i = 0; i < proxyMethods.Count; i++)
            {
                methodMap[proxyMethods[i].Name] = i;
                il.MarkLabel(jmpLabels[i]);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Call, proxyMethods[i]);
                il.Emit(OpCodes.Ret);
            }

            il.MarkLabel(defaultLabel);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            var result = implType.CreateType();
            invokeMethod = result.GetMethod(invokeMeth.Name);
            return result;
        }

        public static Type GenerateInvokerProxy<T>()
        {
            var implType = module.DefineType($"{typeof(T).Name}_InvokeProxy", TypeAttributes.Public, null, new[] {typeof(T)});
            var invokeMethod = typeof(SendMessageDelegate).GetMethod("Invoke");

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

            return implType.CreateType();
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