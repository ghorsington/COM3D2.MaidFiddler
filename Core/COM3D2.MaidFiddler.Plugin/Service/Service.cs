using System.Net;
using System.Net.Sockets;
using COM3D2.MaidFiddler.Core.Utils;
using UnityEngine;

namespace COM3D2.MaidFiddler.Core.Service
{
    public partial class Service
    {
        private static readonly IPEndPoint DefaultLoopbackEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
        private readonly MonoBehaviour parent;

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

        public string Version => typeof(Service).Assembly.GetName().Version.ToString();

        public int GetAvailableTcpPort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(DefaultLoopbackEndpoint);
                return ((IPEndPoint) socket.LocalEndPoint).Port;
            }
        }
    }
}