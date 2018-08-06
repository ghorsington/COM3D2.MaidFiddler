using System.Net;
using System.Net.Sockets;
using System.Text;
using COM3D2.MaidFiddler.Core.Utils;
using UnityEngine;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private readonly MonoBehaviour parent;
        private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, port: 0);

        public Service(MonoBehaviour parent)
        {
            this.parent = parent;
            Debugger.WriteLine(LogLevel.Info, "Created a service provider!");

            InitPlayerStatus();
            InitMaidMgr();
            InitMaidStatus();
            InitGameMain();
        }

        public static int GameVersion => (int) typeof(Misc).GetField(nameof(Misc.GAME_VERSION)).GetValue(null);

        public string GetInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Maid Fiddler {MaidFiddlerPlugin.VERSION} running on");
            sb.AppendLine();

            sb.AppendLine($"* COM3D2 {GameVersion}");
            sb.AppendLine("* Known game DLC:");

            foreach (string s in PluginData.GetAllUniqueNames())
                sb.AppendLine($"  - {s}{(PluginData.IsEnabled(s) ? " [ENABLED]" : "")}");

            return sb.ToString();
        }

        public int GetAvailableTcpPort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(DefaultLoopbackEndpoint);
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}