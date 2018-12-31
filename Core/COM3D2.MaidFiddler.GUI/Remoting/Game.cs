using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using COM3D2.MaidFiddler.Common.Service;

namespace COM3D2.MaidFiddler.GUI.Remoting
{
    public static class Game
    {
        private const int PORT = 8899;
        private static readonly string SERVICE_URL = $"tcp://localhost:{PORT}/MFService.rem";
        private static TcpChannel tcpChannel;
        private static bool isInitialized = false;

        public static GameEvents Events { get; private set; }
        public static IMaidFiddlerService Service { get; private set; }

        static Game()
        {
            if(!isInitialized)
                InitializeService();
        }

        public static void InitializeService()
        {
            if (isInitialized)
                return;

            var clientProv = new BinaryClientFormatterSinkProvider();
            var serverProv = new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full};

            tcpChannel = new TcpChannel(new Hashtable {["name"] = "MFClient", ["port"] = 0}, clientProv, serverProv);
            ChannelServices.RegisterChannel(tcpChannel, false);
            RemotingConfiguration.RegisterWellKnownClientType(typeof(IMaidFiddlerService), SERVICE_URL);

            Service = (IMaidFiddlerService) Activator.GetObject(typeof(IMaidFiddlerService), SERVICE_URL);
            Events = new GameEvents();
            Service.AttachEventHandler(Events.EventHandler);
            isInitialized = true;
        }
    }
}