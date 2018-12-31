using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using COM3D2.MaidFiddler.GUI.Remoting;

namespace COM3D2.MaidFiddler.GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            InitializeComponent();
            Game.InitializeService();
            InitEvents();
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string path = Path.GetFullPath(Path.Combine("Sybaris", $"{new AssemblyName(args.Name).Name}.dll"));
            return File.Exists(path) ? Assembly.LoadFile(path) : null;
        }

        private void InitEvents()
        {
            Game.Events.OnTest += (sender, args) => testLabel.Text = "Hello, world!";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game.Service.Debug("[Remote MFService] TESTOOO!");
            var gameInfo = Game.Service.GetGameInfo();

            Game.Service.Debug($"Got game info with {gameInfo.LockableMaidStats.Count} maid stats");
            Game.Service.Debug($"Known personalities: {string.Join(",", gameInfo.Personalities.Select(p => p.UniqueName))}");
        }
    }
}