using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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