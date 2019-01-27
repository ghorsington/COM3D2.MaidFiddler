using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using COM3D2.MaidFiddler.GUI.Remoting;

namespace COM3D2.MaidFiddler.GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FormClosing += OnClosing;

            InitEvents();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
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