using System.Collections.Generic;
using MsgPack.Serialization;

namespace COM3D2.MaidFiddler.Core.IPC
{
    public interface IMessageData { }

    public class Call : IMessageData
    {
        [MessagePackMember(0, Name = "args")]
        public IList<object> Args { get; set; }

        [MessagePackMember(1, Name = "method")]
        public string Method { get; set; }
    }

    public class Response : IMessageData
    {
        [MessagePackMember(0, Name = "result")]
        public object Result { get; set; }
    }

    public class Error : IMessageData
    {
        [MessagePackMember(0, Name = "err_message")]
        public string ErrorMessage { get; set; }

        [MessagePackMember(1, Name = "err_name")]
        public string ErrorName { get; set; }

        [MessagePackMember(2, Name = "stack_trace")]
        public string StackTrace { get; set; }
    }

    public class Ping : IMessageData
    {
        [MessagePackMember(0, Name = "pong")]
        public bool Pong { get; set; }
    }

    public class Message
    {
        [MessagePackMember(0, Name = "data")]
        [MessagePackKnownType("call", typeof(Call))]
        [MessagePackKnownType("response", typeof(Response))]
        [MessagePackKnownType("error", typeof(Error))]
        [MessagePackKnownType("ping", typeof(Ping))]
        public IMessageData Data { get; set; }

        [MessagePackMember(1, Name = "msg_id")]
        public ulong ID { get; set; }
    }
}