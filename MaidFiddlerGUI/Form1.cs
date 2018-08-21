using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZeroRpc.Net;

namespace MaidFiddlerGUI
{
    public partial class Form1 : Form
    {

        Client c = new Client();

        private InvokeMethods.RemoteMethodAsync GetGameInfo;

        public Form1()
        {
            InitializeComponent();

            c.Connect("tcp://127.0.0.1:8899");

            GetGameInfo = c.CreateAsyncDelegate("GetGameInfo");
        }

        public void TestService()
        {

            //c.InvokeAsync("get_Version", new object[0],
            //              (e, r, s) =>
            //              {
            //                  Console.WriteLine($"Got version: {r}");
            //              });


        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetGameInfo((err, result, _) =>
            {
                var dict = result as Dictionary<string, object>;
                Console.WriteLine($"Got it: {result}");

                foreach (var keyValuePair in dict)
                    Console.WriteLine($"{keyValuePair.Key} => {keyValuePair.Value}");
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            c.Dispose();
        }
    }
}
