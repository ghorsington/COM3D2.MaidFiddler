namespace MiniIPC
{
    public delegate byte[] InvokeMethodDelegate(int command, byte[] data, int start);
    public delegate byte[] SendMessageDelegate(string methodName, byte[] data);
}
