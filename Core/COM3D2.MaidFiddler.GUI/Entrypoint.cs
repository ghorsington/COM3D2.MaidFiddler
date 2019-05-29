using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using COM3D2.MaidFiddler.GUI.Remoting;

namespace COM3D2.MaidFiddler.GUI
{
    public static class Entrypoint
    {
        private static Form1 form;
        private static readonly bool guiInitialized = false;
        private static Thread t;


        [DllExport(CallingConvention.StdCall)]
        public static void ShowGUI()
        {
            if (!guiInitialized)
                StartGUIThread();
            else
                form.Invoke((Action) (() => { form.Visible = true; }));
        }

        [DllExport(CallingConvention.StdCall)]
        public static void HideGUI()
        {
            form.Invoke((Action) (() => { form.Visible = false; }));
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.ThreadException += ApplicationOnThreadException;
        }

        private static void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string path = Path.GetFullPath(Path.Combine("Sybaris", $"{new AssemblyName(args.Name).Name}.dll"));
            return File.Exists(path) ? Assembly.LoadFile(path) : null;
        }

        [STAThread]
        private static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);
        }

        private static void StartGUIThread()
        {
            t = new Thread(Run);
            t.Start();
        }
    }
}