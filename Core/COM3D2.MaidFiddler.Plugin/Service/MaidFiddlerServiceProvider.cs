using System.Text;
using COM3D2.MaidFiddler.Plugin.Utils;

namespace COM3D2.MaidFiddler.Plugin.Service
{
    public class MaidFiddlerServiceProvider
    {
        public MaidFiddlerServiceProvider()
        {
            Debugger.WriteLine(LogLevel.Info, "Created a service provider!");
        }

        public static int GameVersion => (int) typeof(Misc).GetField(nameof(Misc.GAME_VERSION)).GetValue(null);

        public void Echo(string str)
        {
            Debugger.WriteLine($"MaidFiddler: {str}");
        }

        public void SetCredits(object credits)
        {
            long.TryParse(credits.ToString(), out long cred);
            GameMain.Instance.CharacterMgr.status.money = cred;
        }

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