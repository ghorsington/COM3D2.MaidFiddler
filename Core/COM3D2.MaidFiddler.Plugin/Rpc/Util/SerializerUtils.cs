using MsgPack.Serialization;

namespace COM3D2.MaidFiddler.Core.Rpc.Util
{
    public static class SerializerUtils
    {
        private static readonly MessagePackSerializer<Message> serializer;

        static SerializerUtils()
        {
            SerializationContext.Default.SerializationMethod = SerializationMethod.Map;
            serializer = MessagePackSerializer.Get<Message>();
        }

        public static byte[] Serialize(Message msg)
        {
            return serializer.PackSingleObject(msg);
        }

        public static Message Deserialize(byte[] data)
        {
            return serializer.UnpackSingleObject(data);
        }
    }
}