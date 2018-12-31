using COM3D2.MaidFiddler.Common.Data;

namespace COM3D2.MaidFiddler.Common.Service
{
    public interface IMaidFiddlerService
    {
        void Debug(string str);
        void AttachEventHandler(IMaidFiddlerEventHandler handler);
        GameInfo GetGameInfo();
    }

    public interface IMaidFiddlerEventHandler
    {
        void Test();
    }
}