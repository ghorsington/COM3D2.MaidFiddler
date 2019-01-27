using MessagePack;

namespace COM3D2.MaidFiddler.Common.IPC {
    [MessagePackObject]
    public class Error
    {
        [Key(0)]
        public string Message { get; set; }

        [Key(1)]
        public string StackTrace { get; set; }
    }
}