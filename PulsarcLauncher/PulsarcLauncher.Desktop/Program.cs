using System;
using PulsarcLauncher.Util;

namespace PulsarcLauncher.Desktop
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Assets.Application.Run(new PulsarcLauncherForm());
        }
    }
}