using System;
using COM3D2.MaidFiddler.Common.Data;
using COM3D2.MaidFiddler.Common.Service;
using COM3D2.MaidFiddler.Core.Utils;

namespace COM3D2.MaidFiddler.Core.Service
{
    public class MaidFiddlerService : IMaidFiddlerService
    {
        private IMaidFiddlerEventHandler eventHandler;
        public event EventHandler GuiHiding;
        public event EventHandler GuiConnected;

        public void AttachEventHandler(IMaidFiddlerEventHandler handler)
        {
            eventHandler = handler;
        }

        public GameInfo GetGameInfo()
        {
            return Utils.COM3D2.GameInfo;
        }

        public void OnGUIHidden()
        {
            GuiHiding?.Invoke(null, EventArgs.Empty);
        }

        public void OnGUIConnected()
        {
            GuiConnected?.Invoke(null, EventArgs.Empty);
        }

        public void Debug(string str)
        {
            Debugger.WriteLine(LogLevel.Info, str);
        }

        public void ShootEvent()
        {
            eventHandler.Test();
        }

        public void TestEvent()
        {
            Debugger.WriteLine(eventHandler.Test2("Hello from GUI!"));
        }
    }
}