namespace COM3D2.MaidFiddler.Common
{
    public interface IMaidFiddlerService
    {
        void Debug(string str);
        void AttachEventHandler(IMaidFiddlerEventHandler handler);
    }

    public interface IMaidFiddlerEventHandler
    {
        void Test();
    }
}