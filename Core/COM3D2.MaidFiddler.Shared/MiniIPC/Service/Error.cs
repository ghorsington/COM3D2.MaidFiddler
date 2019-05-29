using MessagePack;

namespace MiniIPC {
    [MessagePackObject]
    public class Error
    {
        [Key(0)]
        public string Message { get; set; }

        [Key(1)]
        public string StackTrace { get; set; }
    }
}