using System.Text;
using COM3D2.MaidFiddler.Plugin.Utils;
using ZeroRpc.Net;

namespace COM3D2.MaidFiddler.Plugin.Service
{
    public partial class Service
    {

        public Service()
        {
            Debugger.WriteLine(LogLevel.Info, "Created a service provider!");

            InitPlayerStatus();
            InitMaidStatus();
        }

        public static int GameVersion => (int) typeof(Misc).GetField(nameof(Misc.GAME_VERSION)).GetValue(null);

        public string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Maid Fiddler {MaidFiddlerPlugin.VERSION} running on");
            sb.AppendLine();

            sb.AppendLine($"* COM3D2 {GameVersion}");
            sb.AppendLine("* Known game DLC:");

            foreach (string s in PluginData.GetAllUniqueNames())
                sb.AppendLine($"  - {s}{(PluginData.IsEnabled(s) ? " [ENABLED]" : "")}");

            return sb.ToString();
        }
    }
}