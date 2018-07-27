using System;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public class PlayerStatusChangeArgs : EventArgs
    {
        public bool Locked { get; set; }
        public string Status { get; internal set; }
    }

    public static class PlayerStatusHooks
    {
        public static event EventHandler<PlayerStatusChangeArgs> ShouldPropertyChange;
        public static event EventHandler<PlayerStatusChangeArgs> PropertyChanged;

        public static bool OnPropertyChangePrefix(string property)
        {
            var args = new PlayerStatusChangeArgs {Locked = false, Status = property};

            ShouldPropertyChange?.Invoke(null, args);

            return args.Locked;
        }

        public static void OnPropertyChangePostfix(string property)
        {
            PropertyChanged?.Invoke(null, new PlayerStatusChangeArgs {Status = property});
        }
    }
}