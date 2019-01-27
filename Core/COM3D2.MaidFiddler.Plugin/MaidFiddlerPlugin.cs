using System;
using System.IO;
using System.Runtime.InteropServices;
using COM3D2.MaidFiddler.Common.IPC;
using COM3D2.MaidFiddler.Common.Service;
using COM3D2.MaidFiddler.Core.Service;
using COM3D2.MaidFiddler.Core.Utils;
using GearMenu;
using MessagePack;
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
        private MaidFiddlerService serviceImplementation;
        private ServiceHandler<IMaidFiddlerService> serviceHandler;
        private Action ShowGUI;
        private ExchangeMessagingHandlersDelegate ExchangeMessagingHandlers;
        private Action Initialize;

        private HandleMessageDelegate eventMessageHandler;
        private HandleMessageDelegate serviceMessageHandler;

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
            ExchangeMessagingHandlers = DllUtils.GetProcDelegate<ExchangeMessagingHandlersDelegate>(guiDll, nameof(ExchangeMessagingHandlers));

            Debugger.WriteLine(LogLevel.Info, "Initializing services!");
            Initialize();
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
            serviceHandler = new ServiceHandler<IMaidFiddlerService>(serviceImplementation);
            serviceMessageHandler = serviceHandler.HandleMessage;

            Debugger.WriteLine("Exchanging messaging handlers");
            var resPtr = ExchangeMessagingHandlers(Marshal.GetFunctionPointerForDelegate(serviceMessageHandler));

            eventMessageHandler = (HandleMessageDelegate) Marshal.GetDelegateForFunctionPointer(resPtr, typeof(HandleMessageDelegate));
            serviceImplementation.AttachEventHandler(ServiceProxyGenerator.GenerateServiceProxy<IMaidFiddlerEventHandler>(SendMessage));

            Debugger.WriteLine("Testing the event!");
            serviceImplementation.TestEvent();
            Debugger.WriteLine("Test complete!");
        }

        private byte[] SendMessage(string methodname, byte[] data)
        {
            byte[] result = null;
            Error err = null;
            unsafe
            {
                fixed (byte* b = data)
                {
                    var res = eventMessageHandler(methodname, new IntPtr(b), data?.Length ?? 0, out var len);
                    if (res == IntPtr.Zero)
                        return null;

                    var d = (byte*) res;
                    if (len == 0 && *d == 0x00)
                    {
                        using (var us = new UnmanagedMemoryStream(d + 1, len))
                            err = MessagePackSerializer.Deserialize<Error>(us);
                    }
                    else
                    {
                        result = new byte[len];
                        Marshal.Copy(res, result, 0, (int)len);
                    }
                    Marshal.FreeHGlobal(res);
                }
            }

            if(err != null)
                throw new RemoteException(err.Message, err.StackTrace);
            return result;
        }

        private void OnGuiHiding(object sender, EventArgs e)
        {
            UpdateGearButton(false);
        }
    }
}