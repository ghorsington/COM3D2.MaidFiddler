using System;
using System.IO;
using System.Runtime.InteropServices;
using COM3D2.MaidFiddler.Core.IPC;
using COM3D2.MaidFiddler.Core.Utils;
using UnityEngine;
using UnityInjector;
using MFService = COM3D2.MaidFiddler.Core.Service.Service;

namespace COM3D2.MaidFiddler.Core
{
    public class MaidFiddlerPlugin : PluginBase
    {
        private PipeService<MFService> pipeServer;
        private MFService service;

        internal string Version { get; } = typeof(MaidFiddlerPlugin).Assembly.GetName().Version.ToString();

        private string dllPath;
        private IntPtr guiDll;

        private Action StartGUIThread;
        private Action HideGUI;
        private Action ShowGUI;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            dllPath = Path.Combine(Directory.GetParent(Path.GetFullPath(DataPath)).Parent.FullName, "COM3D2.MaidFiddler.GUI.dll");
            Debug.Log($"[MF] DLL Path: {dllPath}");

            if (!File.Exists(dllPath))
            {
                Debug.Log("[MF] No GUI dll found!");
                Destroy(this);
                return;
            }

            guiDll = DllUtils.LoadLibrary(dllPath);

            StartGUIThread = DllUtils.GetProcDelegate<Action>(guiDll, nameof(StartGUIThread));
            HideGUI = DllUtils.GetProcDelegate<Action>(guiDll, nameof(HideGUI));
            ShowGUI = DllUtils.GetProcDelegate<Action>(guiDll, nameof(ShowGUI));

            //Debugger.WriteLine(LogLevel.Info, $"Starting up Maid Fiddler {Version}");

            //service = new MFService(this);
            //service.eventServer.ConnectionLost += OnConnectionLost;

            //Debugger.WriteLine(LogLevel.Info, "Starting server!");

            //pipeServer = new PipeService<MFService>(service, "MaidFiddlerService");
            //pipeServer.ConnectionLost += OnConnectionLost;
            //pipeServer.Run();

            //Debugger.WriteLine(LogLevel.Info, "Started server!");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("[MF] Starting GUI!");
                StartGUIThread();
                //ShowGUI();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("[MF] Hiding GUI!");
                HideGUI();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("[MF] Showing GUI!");
                ShowGUI();
            }
        }

        public void OnDestroy()
        {
            Debugger.WriteLine(LogLevel.Info, "Stopping MaidFiddler");

            DllUtils.FreeLibrary(guiDll);
            
            //try
            //{
            //    pipeServer.Dispose();
            //}
            //catch (Exception)
            //{
            //    // Snibbeti snib :--D
            //}

            Debugger.WriteLine(LogLevel.Info, "Maid Fiddler stopped!");
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            //if (service.eventServer.IsConnected)
            //    service.eventServer.Disconnect();
            //service.eventServer.WaitForConnection();
        }
    }
}