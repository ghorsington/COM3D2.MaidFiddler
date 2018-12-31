using System;
using COM3D2.MaidFiddler.Common.Data;
using COM3D2.MaidFiddler.Common.Service;
using COM3D2.MaidFiddler.Core.Utils;

namespace COM3D2.MaidFiddler.Core.Service
{
    public class MaidFiddlerService : MarshalByRefObject, IMaidFiddlerService
    {
        private IMaidFiddlerEventHandler eventHandler;

        public void AttachEventHandler(IMaidFiddlerEventHandler handler)
        {
            eventHandler = handler;
        }

        public GameInfo GetGameInfo()
        {
            return Utils.COM3D2.GameInfo;
        }

        public void Debug(string str)
        {
            Debugger.WriteLine(LogLevel.Info, str);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void ShootEvent()
        {
            eventHandler.Test();
        }
    }
}