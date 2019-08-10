using System;
using COM3D2.MaidFiddler.Core.IPC;
using COM3D2.MaidFiddler.Core.Utils;
using UnityInjector;
using MFService = COM3D2.MaidFiddler.Core.Service.Service;

namespace COM3D2.MaidFiddler.Core
{
    public class MaidFiddlerPlugin : PluginBase
    {
        private PipeService<MFService> pipeServer;
        private MFService service;

        internal string Version { get; } = typeof(MaidFiddlerPlugin).Assembly.GetName().Version.ToString();

        public void Awake()
        {
            DontDestroyOnLoad(this);

            Debugger.WriteLine(LogLevel.Info, $"Starting up Maid Fiddler {Version}");

            service = new MFService(this);
            service.eventServer.ConnectionLost += OnConnectionLost;

            Debugger.WriteLine(LogLevel.Info, "Starting server!");

            pipeServer = new PipeService<MFService>(service, "MaidFiddlerService");
            pipeServer.ConnectionLost += OnConnectionLost;
            pipeServer.Run();

            Debugger.WriteLine(LogLevel.Info, "Started server!");
        }

        public void LateUpdate()
        {
            service?.UpdateActiveMaidStatus();
        }

        public void OnDestroy()
        {
            Debugger.WriteLine(LogLevel.Info, "Stopping MaidFiddler");

            try
            {
                pipeServer.Dispose();
            }
            catch (Exception)
            {
                // Snibbeti snib :--D
            }

            Debugger.WriteLine(LogLevel.Info, "Maid Fiddler stopped!");
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            if (service.eventServer.IsConnected)
                service.eventServer.Disconnect();
            service.eventServer.WaitForConnection();
        }
    }
}