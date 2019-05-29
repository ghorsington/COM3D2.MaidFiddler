using System;
using COM3D2.MaidFiddler.Common.Service;
using GhettoPipes;
using MiniIPC.Service;

namespace COM3D2.MaidFiddler.GUI.Remoting
{
    public static class Game
    {
        private static bool isInitialized = false;

        private static NamedPipeStream eventReceiverPipe;
        private static NamedPipeStream serviceSenderPipe;
        private static StreamServiceSender<IMaidFiddlerService> serviceSender;
        public static GameEvents Events { get; private set; } = new GameEvents();
        public static IMaidFiddlerService Service { get; private set; }
        public static StreamServiceReceiver<IMaidFiddlerEventHandler> EventService { get; private set; }

        public static string InitializeConnection()
        {
            if (serviceSender != null)
                return "";
            try
            {
                serviceSenderPipe = NamedPipeStream.Open("MaidFiddler_GameService", NamedPipeStream.PipeDirection.InOut);
                serviceSender = new StreamServiceSender<IMaidFiddlerService>(serviceSenderPipe);
                Service = serviceSender.Service;
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static void InitializeService()
        {
            if (isInitialized)
                return;
            
            eventReceiverPipe = NamedPipeStream.Create("MaidFiddler_EventService", NamedPipeStream.PipeDirection.InOut, securityDescriptor: "D:(A;OICI;GA;;;WD)");
            EventService = new StreamServiceReceiver<IMaidFiddlerEventHandler>(Events.EventHandler, eventReceiverPipe);
            Service.OnGUIConnected();
            isInitialized = true;
        }

        private static void ReceiveLoop()
        {
            eventReceiverPipe.WaitForConnection();

            while (true)
            {
                EventService.ProcessMessage();
                eventReceiverPipe.Flush();
            }
        }
    }
}