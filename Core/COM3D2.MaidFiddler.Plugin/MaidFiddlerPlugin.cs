using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using COM3D2.MaidFiddler.Core.Service;
using COM3D2.MaidFiddler.Core.Utils;
using UnityEngine;
using UnityInjector;

namespace COM3D2.MaidFiddler.Core
{
    public class MaidFiddlerPlugin : PluginBase
    {
        private const int PORT = 8899;
        private const string SERVICE_NAME = "MFService.rem";

        private string dllPath;
        private IntPtr guiDll;
        private MaidFiddlerService serviceImplementation;
        private Action HideGUI;
        private Action ShowGUI;
        private ObjRef internalObj;
        private TcpServerChannel tcpChannel;

        internal string Version { get; } = typeof(MaidFiddlerPlugin).Assembly.GetName().Version.ToString();

        public void Awake()
        {
            DontDestroyOnLoad(this);
            dllPath = Path.Combine(Directory.GetParent(Path.GetFullPath(DataPath)).Parent.FullName, "COM3D2.MaidFiddler.GUI.dll");
            Debugger.WriteLine(LogLevel.Info, $"Starting up Maid Fiddler {Version}");

            if (!File.Exists(dllPath))
            {
                Debugger.WriteLine(LogLevel.Warning, $"No GUI DLL found! Check that {dllPath} exists!");
                Destroy(this);
                return;
            }

            guiDll = DllUtils.LoadLibrary(dllPath);
            HideGUI = DllUtils.GetProcDelegate<Action>(guiDll, nameof(HideGUI));
            ShowGUI = DllUtils.GetProcDelegate<Action>(guiDll, nameof(ShowGUI));

            Debugger.WriteLine(LogLevel.Info, "Initializing TCP service!");
            InitService();
            Debugger.WriteLine(LogLevel.Info, "Service initialized!");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debugger.Debug(LogLevel.Info, "Showing GUI!");
                ShowGUI();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Debugger.Debug(LogLevel.Info, "Hiding GUI!");
                HideGUI();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                serviceImplementation.ShootEvent();
            }
        }

        public void OnDestroy()
        {
            Debugger.WriteLine(LogLevel.Info, "Stopping MaidFiddler");

            RemotingServices.Unmarshal(internalObj);
            ChannelServices.UnregisterChannel(tcpChannel);

            DllUtils.FreeLibrary(guiDll);

            Debugger.WriteLine(LogLevel.Info, "Maid Fiddler stopped!");
        }

        private void InitService()
        {
            serviceImplementation = new MaidFiddlerService();
            var serverProv = new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full};
            tcpChannel = new TcpServerChannel(new Hashtable
            {
                ["port"] = PORT,
                ["name"] = SERVICE_NAME
            }, serverProv);
            ChannelServices.RegisterChannel(tcpChannel, false);
            internalObj = RemotingServices.Marshal(serviceImplementation, SERVICE_NAME);
        }
    }
}