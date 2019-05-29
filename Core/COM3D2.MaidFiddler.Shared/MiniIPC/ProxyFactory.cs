using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiniIPC
{
    public static class ProxyFactory
    {
        private static readonly Dictionary<Type, Type> proxyImplementations = new Dictionary<Type, Type>();

        private static readonly Dictionary<Type, Dictionary<string, int>> receiverProxyCommandMaps =
                new Dictionary<Type, Dictionary<string, int>>();

        private static readonly Dictionary<Type, Type> receiverProxyImplementations = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, MethodInfo> receiverProxyInvokeMethods = new Dictionary<Type, MethodInfo>();

        public static InvokeMethodDelegate CreateReceiverProxy<T>(T service, out object instance, out Dictionary<string, int> methodMap)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("The given type must be an interface");

            if (!receiverProxyImplementations.TryGetValue(typeof(T), out var proxyType))
            {
                proxyType = DynamicCodeGenerator.MakeReceiverProxy<T>(out var methodRef, out var commandMap);
                receiverProxyImplementations[typeof(T)] = proxyType;
                receiverProxyInvokeMethods[typeof(T)] = methodRef;
                receiverProxyCommandMaps[typeof(T)] = commandMap;
            }

            instance = Activator.CreateInstance(proxyType, service);
            methodMap = new Dictionary<string, int>(receiverProxyCommandMaps[typeof(T)]);
            return (InvokeMethodDelegate) Delegate.CreateDelegate(typeof(InvokeMethodDelegate),
                                                                  instance,
                                                                  receiverProxyInvokeMethods[typeof(T)]);
        }

        public static T CreateSenderProxy<T>(SendMessageDelegate sendMessage)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("The given type must be an interface");

            if (!proxyImplementations.TryGetValue(typeof(T), out var proxyImplementation))
            {
                proxyImplementation = DynamicCodeGenerator.GenerateInvokerProxy<T>();
                proxyImplementations[typeof(T)] = proxyImplementation;
            }

            return (T) Activator.CreateInstance(proxyImplementation, sendMessage);
        }
    }
}