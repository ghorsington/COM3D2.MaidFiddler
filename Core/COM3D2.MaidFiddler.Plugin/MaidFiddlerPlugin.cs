using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using COM3D2.MaidFiddler.Core.Service;
using COM3D2.MaidFiddler.Core.Utils;
using GearMenu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;

namespace COM3D2.MaidFiddler.Core
{
    public class MaidFiddlerPlugin : PluginBase
    {
        private const int PORT = 8899;
        private const string SERVICE_NAME = "MFService.rem";
        private readonly Color DisabledColor = new Color(0.827f, 0.827f, 0.827f);

        private string dllPath;
        private readonly Color EnabledColor = Color.red;
        private byte[] gearIcon;

        private bool guiDisplayed;
        private IntPtr guiDll;
        private Action HideGUI;
        private ObjRef internalObj;
        private MaidFiddlerService serviceImplementation;
        private Action ShowGUI;
        private TcpServerChannel tcpChannel;

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

            Debugger.WriteLine(LogLevel.Info, "Initializing TCP service!");
            InitService();
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

            RemotingServices.Unmarshal(internalObj);
            ChannelServices.UnregisterChannel(tcpChannel);
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
            serviceImplementation = new MaidFiddlerService();
            serviceImplementation.GuiHiding += OnGuiHiding;
            var serverProv = new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full};
            tcpChannel = new TcpServerChannel(new Hashtable {["port"] = PORT, ["name"] = SERVICE_NAME}, serverProv);
            ChannelServices.RegisterChannel(tcpChannel, false);
            internalObj = RemotingServices.Marshal(serviceImplementation, SERVICE_NAME);
        }

        private void OnGuiHiding(object sender, EventArgs e)
        {
            UpdateGearButton(false);
        }
    }
}