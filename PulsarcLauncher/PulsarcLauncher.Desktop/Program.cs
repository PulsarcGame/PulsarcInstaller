using System;
using Eto.Forms;
using Eto.Drawing;

namespace PulsarcLauncher.Desktop
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Application(Eto.Platform.Detect).Run(new PulsarcLauncherForm());
        }
    }
}