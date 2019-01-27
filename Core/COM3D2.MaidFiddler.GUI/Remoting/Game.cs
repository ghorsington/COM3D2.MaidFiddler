using System;
using System.IO;
using System.Runtime.InteropServices;
using COM3D2.MaidFiddler.Common.IPC;
using COM3D2.MaidFiddler.Common.Service;
using MessagePack;

namespace COM3D2.MaidFiddler.GUI.Remoting
{
    public static class Game
    {
        private static bool isInitialized = false;

        public static GameEvents Events { get; private set; }
        public static IMaidFiddlerService Service { get; private set; }
        private static HandleMessageDelegate HandleMessage { get; set; }
        public static ServiceHandler<IMaidFiddlerEventHandler> EventService { get; private set; }

        public static void InitializeService(HandleMessageDelegate messageHandler)
        {
            if (isInitialized)
                return;
            HandleMessage = messageHandler;
            Service = ServiceProxyGenerator.GenerateServiceProxy<IMaidFiddlerService>(SendMessage);
            Events = new GameEvents();
            EventService = new ServiceHandler<IMaidFiddlerEventHandler>(Events.EventHandler);
            isInitialized = true;
        }

        private static byte[] SendMessage(string methodName, byte[] data)
        {
            byte[] result = null;
            Error err = null;
            unsafe
            {
                fixed (byte* b = data)
                {
                    var res = HandleMessage(methodName, new IntPtr(b), data?.Length ?? 0, out var len);
                    if (res == IntPtr.Zero)
                        return null;

                    var d = (byte*)res;
                    if (len == 0 && *d == 0x00)
                    {
                        using (var us = new UnmanagedMemoryStream(d + 1, len))
                            err = MessagePackSerializer.Deserialize<Error>(us);
                    }
                    else
                    {
                        result = new byte[len];
                        Marshal.Copy(res, result, 0, (int)len);
                    }
                    Marshal.FreeHGlobal(res);
                }
            }

            if (err != null)
                throw new RemoteException(err.Message, err.StackTrace);
            return result;
        }
    }
}