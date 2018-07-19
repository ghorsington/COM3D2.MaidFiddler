using System;

namespace COM3D2.MaidFiddler.Core.Hooks
{
    public class MaidChangeEventArgs : EventArgs
    {
        public Maid Maid { get; internal set; }
    }

    public static class CharacterMgrHooks
    {
        public static event EventHandler<MaidChangeEventArgs> MaidBanished;
        public static event EventHandler<MaidChangeEventArgs> MaidAdded;

        public static void OnCharacterBanished(Maid maid, bool isMan)
        {
            if (isMan)
                return;

            MaidBanished?.Invoke(null, new MaidChangeEventArgs {Maid = maid});
        }

        public static void OnCharacterAdded(ref Maid maid, bool isMan, bool isNpc)
        {
            if (isMan || isNpc)
                return;

            MaidAdded?.Invoke(null, new MaidChangeEventArgs {Maid = maid});
        }
    }
}