using System;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public class DeserializeEventArgs : EventArgs
    {
        public bool Success { get; internal set; }
    }

    public static class GameMainHooks
    {
        public static event EventHandler DeserializeStarting;
        public static event EventHandler<DeserializeEventArgs> DeserializeEnded; 

        public static void OnPreDeserialize()
        {
            DeserializeStarting?.Invoke(null, new EventArgs());
        }

        public static bool OnPostDeserialize(bool success)
        {
            DeserializeEnded?.Invoke(null, new DeserializeEventArgs
            {
                Success = success
            });

            return success;
        }
    }
}
