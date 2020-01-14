using System;
using System.Diagnostics;
using System.Threading.Tasks;
using PulsarcInstaller.Util;

namespace PulsarcInstaller.Desktop
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Assets.Application.Run(new PulsarcInstallerForm()));
        }
    }
}