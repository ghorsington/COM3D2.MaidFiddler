using System;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public class DeserializeEventArgs : EventArgs
    {
        public bool Success { get; internal set; }
    }

    public static class GameMainHooks
    {
        public static bool IsDeserializing { get; private set; }
        public static event EventHandler DeserializeStarting;
        public static event EventHandler<DeserializeEventArgs> DeserializeEnded;

        public static void OnPreDeserialize()
        {
            IsDeserializing = true;
            DeserializeStarting?.Invoke(null, new EventArgs());
        }

        public static bool OnPostDeserialize(bool success)
        {
            IsDeserializing = false;
            DeserializeEnded?.Invoke(null, new DeserializeEventArgs {Success = success});

            return success;
        }
    }
}