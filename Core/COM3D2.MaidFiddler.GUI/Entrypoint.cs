using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COM3D2.MaidFiddler.GUI
{
    public static class Entrypoint
    {
        private static Form1 form;
        private static Thread t;

        [STAThread]
        private static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void ShowGUI()
        {
            form.Invoke((Action)(() => { form.Visible = true; }));
        }

        [DllExport(CallingConvention.StdCall)]
        public static void HideGUI()
        {
            form.Invoke((Action)(() => { form.Visible = false; }));
        }

        [DllExport(CallingConvention.StdCall)]
        public static void StartGUIThread()
        {
            t = new Thread(Run);
            t.Start();
        }
    }
}
