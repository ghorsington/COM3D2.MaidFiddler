using System;
using AsyncIO;
using COM3D2.MaidFiddler.Plugin.Service;
using COM3D2.MaidFiddler.Plugin.Utils;
using NetMQ;
using UnityInjector;
using UnityInjector.Attributes;
using ZeroRpc.Net;
using ZeroRpc.Net.ServiceProviders;
using MFService = COM3D2.MaidFiddler.Plugin.Service.Service;

namespace COM3D2.MaidFiddler.Plugin
{
    [PluginName("COM3D2.MaidFiddler")]
    [PluginVersion(VERSION)]
    public class MaidFiddlerPlugin : PluginBase
    {
        public const int PORT = 8899;
        public const string VERSION = "Alpha 0.1";

        private Server zeroServer;
        private MFService service;

        public void Awake()
        {
            DontDestroyOnLoad(this);

            ForceDotNet.Force();
            NetMQConfig.Linger = TimeSpan.Zero;

            Debugger.WriteLine(LogLevel.Info, $"Starting up Maid Fiddler {VERSION}");

            service = new MFService();

            Debugger.WriteLine(LogLevel.Info, $"Creating a ZeroService at tcp://localhost:{PORT}");

            zeroServer = new Server(new SimpleWrapperService<MFService>(service));

            zeroServer.Error += (sender, args) =>
            {
                Debugger.WriteLine(LogLevel.Error, $"ZeroService error: {args.Info.ToString()}");
            };

            Debugger.WriteLine(LogLevel.Info, "Starting server!");

            zeroServer.Bind($"tcp://localhost:{PORT}");

            Debugger.WriteLine(LogLevel.Info, "Started server!");
        }

        public void OnDestroy()
        {
            Debugger.WriteLine(LogLevel.Info, "Stopping ZeroService");
            service.Unsubscribe();
            zeroServer.Dispose();
            Debugger.WriteLine(LogLevel.Info, "Doing cleanup!");
            NetMQConfig.Cleanup(false);
            Debugger.WriteLine(LogLevel.Info, "Cleanup started!");
        }
    }
}