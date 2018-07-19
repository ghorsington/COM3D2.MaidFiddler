using System;
using AsyncIO;
using BepInEx;
using COM3D2.MaidFiddler.Core.Utils;
using NetMQ;
using ZeroRpc.Net;
using ZeroRpc.Net.ServiceProviders;
using MFService = COM3D2.MaidFiddler.Core.Service.Service;

namespace COM3D2.MaidFiddler.Core
{
    [BepInPlugin("horse.coder.com3d2.maidfiddler", "Maid Fiddler for COM3D2", "0.0.1")]
    public class MaidFiddlerPlugin : BaseUnityPlugin
    {
        public const string IP = "127.0.0.1";
        public const int PORT = 8899;
        public const string VERSION = "Alpha 0.1";
        private MFService service;

        private Server zeroServer;

        public void Awake()
        {
            DontDestroyOnLoad(this);

            ForceDotNet.Force();
            NetMQConfig.Linger = TimeSpan.Zero;

            Debugger.WriteLine(LogLevel.Info, $"Starting up Maid Fiddler {VERSION}");

            service = new MFService(this);

            Debugger.WriteLine(LogLevel.Info, $"Creating a ZeroService at tcp://{IP}:{PORT}");

            zeroServer = new Server(new SimpleWrapperService<MFService>(service), TimeSpan.FromSeconds(15));

            zeroServer.Error += (sender, args) => { Debugger.WriteLine(LogLevel.Error, $"ZeroService error: {args.Info.ToString()}"); };

            Debugger.WriteLine(LogLevel.Info, "Starting server!");

            zeroServer.Bind($"tcp://{IP}:{PORT}");

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
                //
            }

            Debugger.WriteLine(LogLevel.Info, "Doing cleanup!");
            NetMQConfig.Cleanup(false);
            Debugger.WriteLine(LogLevel.Info, "Cleanup started!");
        }
    }
}