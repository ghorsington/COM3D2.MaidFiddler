using COM3D2.MaidFiddler.Common.Data;

namespace COM3D2.MaidFiddler.Common.Service
{
    public interface IMaidFiddlerService
    {
        void Debug(string str);
        GameInfo GetGameInfo();
        void OnGUIHidden();
        void OnGUIConnected();
    }

    public interface IMaidFiddlerEventHandler
    {
        void Test();
        string Test2(string foo);
    }
}