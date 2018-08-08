using System;
using AsyncIO;
using COM3D2.MaidFiddler.Core.Utils;
using NetMQ;
using UnityInjector;
using ZeroRpc.Net;
using ZeroRpc.Net.ServiceProviders;
using MFService = COM3D2.MaidFiddler.Core.Service.Service;

namespace COM3D2.MaidFiddler.Core
{
    public class MaidFiddlerPlugin : PluginBase
    {
        public const int DEFAULT_PORT = 8899;
        public const string IP = "127.0.0.1";

        private MFService service;
        private Server zeroServer;

        internal string Version { get; } = typeof(MaidFiddlerPlugin).Assembly.GetName().Version.ToString();

        public void Awake()
        {
            DontDestroyOnLoad(this);

            if (!int.TryParse(Preferences["Connection"]["port"].Value, out int connectionPort) || connectionPort < 0
                                                                                               || connectionPort > ushort.MaxValue)
            {
                connectionPort = DEFAULT_PORT;
                Preferences["Connection"]["port"].Value = DEFAULT_PORT.ToString();
                SaveConfig();
            }

            ForceDotNet.Force();
            NetMQConfig.Linger = TimeSpan.Zero;

            Debugger.WriteLine(LogLevel.Info, $"Starting up Maid Fiddler {Version}");

            service = new MFService(this);

            Debugger.WriteLine(LogLevel.Info, $"Creating a ZeroService at tcp://{IP}:{connectionPort}");

            zeroServer = new Server(new SimpleWrapperService<MFService>(service), TimeSpan.FromSeconds(15));

            zeroServer.Error += (sender, args) => { Debugger.WriteLine(LogLevel.Error, $"ZeroService error: {args.Info.ToString()}"); };

            Debugger.WriteLine(LogLevel.Info, "Starting server!");

            zeroServer.Bind($"tcp://{IP}:{connectionPort}");

            Debugger.WriteLine(LogLevel.Info, "Started server!");
        }

        public void OnDestroy()
        {
            Debugger.WriteLine(LogLevel.Info, "Stopping ZeroService");

            try
            {
                service.StopEventEmitter();
                zeroServer.Dispose();
            }
            catch (Exception)
            {
                // Snibbeti snib :--D
            }

            Debugger.WriteLine(LogLevel.Info, "Doing cleanup!");
            NetMQConfig.Cleanup(false);
        }
    }
}