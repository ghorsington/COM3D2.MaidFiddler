using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Windows.Forms;
using COM3D2.MaidFiddler.Common;

namespace COM3D2.MaidFiddler.GUI
{
    public partial class Form1 : Form
    {
        private const int PORT = 8899;
        private readonly string SERVICE_URL = $"tcp://localhost:{PORT}/MFService.rem";
        private IMaidFiddlerService service;
        private IMaidFiddlerEventHandler test;
        private TcpChannel tcpChannel;

        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            InitializeComponent();
            InitService();
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string path = Path.GetFullPath(Path.Combine("Sybaris", $"{new AssemblyName(args.Name).Name}.dll"));
            return File.Exists(path) ? Assembly.LoadFile(path) : null;
        }

        private void InitService()
        {
            var clientProv = new BinaryClientFormatterSinkProvider();
            var serverProv = new BinaryServerFormatterSinkProvider{ TypeFilterLevel = TypeFilterLevel.Full};

            tcpChannel = new TcpChannel(new Hashtable
            {
                ["name"] = "MFClient",
                ["port"] = 0
            }, clientProv, serverProv);
            ChannelServices.RegisterChannel(tcpChannel, false);
            RemotingConfiguration.RegisterWellKnownClientType(typeof(IMaidFiddlerService), SERVICE_URL);
            service = (IMaidFiddlerService) Activator.GetObject(typeof(IMaidFiddlerService), SERVICE_URL);
            test = new EventHandlerTest();
            service.AttachEventHandler(test);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            service.Debug("[Remote MFService] TESTOOO!");
        }
    }

    public class EventHandlerTest : MarshalByRefObject, IMaidFiddlerEventHandler
    {
        public void Test()
        {
            MessageBox.Show("Test!");
        }

        public override object InitializeLifetimeService() => null;
    }
}