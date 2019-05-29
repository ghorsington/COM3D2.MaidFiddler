using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using COM3D2.MaidFiddler.Common.Service;
using COM3D2.MaidFiddler.Core.Service;
using COM3D2.MaidFiddler.Core.Utils;
using GearMenu;
using GhettoPipes;
using MessagePack;
using MiniIPC;
using MiniIPC.Service;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;

namespace COM3D2.MaidFiddler.Core
{
    public class MaidFiddlerPlugin : PluginBase
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr ExchangeMessagingHandlersDelegate(IntPtr plugin);

        private readonly Color DisabledColor = new Color(0.827f, 0.827f, 0.827f);

        private string dllPath;
        private readonly Color EnabledColor = Color.red;
        private byte[] gearIcon;

        private bool guiDisplayed;
        private IntPtr guiDll;
        private Action HideGUI;
        private Action ShowGUI;
        private Action Initialize;

        private MaidFiddlerService serviceImplementation;
        private NamedPipeStream receivePipeStream;
        private StreamServiceReceiver<IMaidFiddlerService> serviceReceiver;
        private Thread receiverThread;

        private NamedPipeStream sendPipeStream;
        private StreamServiceSender<IMaidFiddlerEventHandler> serviceSender;

        internal string Version { get; } = typeof(MaidFiddlerPlugin).Assembly.GetName().Version.ToString();
        private string ButtonLabel => $"MaidFiddler {(guiDisplayed ? "OFF" : "ON")}";

        private string ButtonName => "MaidFiddlerGearButton";

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
            Initialize = DllUtils.GetProcDelegate<Action>(guiDll, nameof(Initialize));
            //ExchangeMessagingHandlers = DllUtils.GetProcDelegate<ExchangeMessagingHandlersDelegate>(guiDll, nameof(ExchangeMessagingHandlers));

            Debugger.WriteLine(LogLevel.Info, "Initializing services!");
            InitService();
            Initialize();
            Debugger.WriteLine(LogLevel.Info, "Service initialized!");

            gearIcon = GetType().Assembly.GetResourceBytes("Icon/GearMenuIcon.png");

            SceneManager.sceneLoaded += (arg0, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                    DisplayGearButton();
            };
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.A) && !guiDisplayed)
            {
                Debugger.Debug(LogLevel.Info, "Showing GUI!");
                ShowGUI();
                UpdateGearButton(true);
            }
            else if (Input.GetKeyDown(KeyCode.S) && guiDisplayed)
            {
                Debugger.Debug(LogLevel.Info, "Hiding GUI!");
                HideGUI();
                UpdateGearButton(false);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                serviceImplementation.ShootEvent();
            }
        }

        public void OnDestroy()
        {
            Debugger.WriteLine(LogLevel.Info, "Stopping MaidFiddler");

            DllUtils.FreeLibrary(guiDll);

            Debugger.WriteLine(LogLevel.Info, "Maid Fiddler stopped!");
        }

        private void OnGearMenuClick(GameObject go)
        {
            UpdateGearButton(!guiDisplayed);

            if (guiDisplayed)
                ShowGUI();
            else
                HideGUI();
        }

        private void DisplayGearButton()
        {
            if (Buttons.Contains(ButtonName))
                Buttons.Remove(ButtonName);

            Buttons.Add(ButtonName, ButtonLabel, gearIcon, OnGearMenuClick);
            Buttons.SetFrameColor(ButtonName, guiDisplayed ? EnabledColor : DisabledColor);
        }

        private void UpdateGearButton(bool enable)
        {
            guiDisplayed = enable;
            Buttons.Add(ButtonName, ButtonLabel, gearIcon, OnGearMenuClick);
            Buttons.SetFrameColor(ButtonName, guiDisplayed ? EnabledColor : DisabledColor);
        }

        private void InitService()
        {
            receivePipeStream = NamedPipeStream.Create("MaidFiddler_GameService", NamedPipeStream.PipeDirection.InOut, securityDescriptor: "D:(A;OICI;GA;;;WD)");
            serviceImplementation = new MaidFiddlerService();
            serviceImplementation.GuiHiding += OnGuiHiding;
            serviceImplementation.GuiConnected += OnGUIConnected;
            serviceReceiver = new StreamServiceReceiver<IMaidFiddlerService>(serviceImplementation, receivePipeStream);
            receiverThread = new Thread(ReceiverLoop);
            receiverThread.Start();
        }

        private void OnGUIConnected(object sender, EventArgs e)
        {
            Debugger.WriteLine("GUI Connected and done! Creating service!");

            sendPipeStream = NamedPipeStream.Open("MaidFiddler_EventService", NamedPipeStream.PipeDirection.InOut);
            serviceSender = new StreamServiceSender<IMaidFiddlerEventHandler>(sendPipeStream);

            serviceImplementation.AttachEventHandler(serviceSender.Service);
        }

        private void ReceiverLoop()
        {
            Debugger.WriteLine($"Started waiting for connections...");
            receivePipeStream.WaitForConnection();
            Debugger.WriteLine($"Got connection! Processing messages!");

            while (true)
            {
                serviceReceiver.ProcessMessage();
                receivePipeStream.Flush();
            }
        }

        private void OnGuiHiding(object sender, EventArgs e)
        {
            UpdateGearButton(false);
        }
    }
}