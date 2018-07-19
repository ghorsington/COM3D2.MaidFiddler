using COM3D2.MaidFiddler.Core.Hooks;
using COM3D2.MaidFiddler.Core.Utils;
using Dict = System.Collections.Generic.Dictionary<string, object>;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private void InitMaidMgr()
        {
            CharacterMgrHooks.MaidAdded += OnMaidAdded;
            CharacterMgrHooks.MaidBanished += OnMaidBanished;
        }

        private void OnMaidBanished(object sender, MaidChangeEventArgs e)
        {
            if (!HasEventHandler || IsDeserializing)
                return;

            Emit("maid_removed", new Dict {["maid_id"] = e.Maid.status.guid});

            AddMaid(e.Maid);
        }

        private void OnMaidAdded(object sender, MaidChangeEventArgs e)
        {
            if (!HasEventHandler || IsDeserializing)
                return;

            Debugger.WriteLine(LogLevel.Info, $"Got maid: {e.Maid}");

            Emit("maid_added", new Dict {["maid"] = ReadBasicMaidData(e.Maid)});

            RemoveMaid(e.Maid);
        }
    }
}