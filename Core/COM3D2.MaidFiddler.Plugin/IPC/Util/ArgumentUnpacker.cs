using System.Collections.Generic;
using System.Linq;
using MsgPack;

namespace COM3D2.MaidFiddler.Core.IPC.Util
{
    public static class ArgumentUnpacker
    {
        public static object Unpack(MessagePackObject obj)
        {
            if (obj.IsList)
            {
                var list = obj.AsList();
                return Unpack(list);
            }

            if (obj.IsMap)
            {
                MessagePackObjectDictionary map = obj.AsDictionary();
                return map.ToDictionary(k => k.Key.ToString(), k => Unpack(k.Value));
            }

            return obj.ToObject();
        }

        public static object[] Unpack(IList<object> list)
        {
            var result = new object[list.Count];

            for (int i = 0; i < list.Count; i++)
                result[i] = Unpack((MessagePackObject) list[i]);

            return result;
        }

        public static object[] Unpack(IList<MessagePackObject> list)
        {
            var result = new object[list.Count];

            for (int i = 0; i < list.Count; i++)
                result[i] = Unpack(list[i]);

            return result;
        }
    }
}