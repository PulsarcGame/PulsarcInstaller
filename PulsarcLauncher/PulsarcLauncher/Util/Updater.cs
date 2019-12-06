using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace PulsarcLauncher.Util
{
    public static class Updater
    {
        public static bool IsOnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool Is64Bit => Environment.Is64BitProcess;
        // %appdata% folder
        private const string DEFAULT_WINDOWS_FOLDER_NAME = ".pulsarc";
        private static readonly string DefaultWindowsInstallPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace('\\', '/')
            + '/' + DEFAULT_WINDOWS_FOLDER_NAME;

        public static bool IsOnMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        private const string DEFAULT_MAC_FOLDER_NAME = "Pulsarc";
        private static string DefaultMacInstallPath = "" + '/' + DEFAULT_MAC_FOLDER_NAME; // TODO

        public static bool IsOnLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private const string DEFAULT_LINUX_FOLDER_NAME = "Pulsarc";
        private static string DefaultLinuxInstallPath = "" + '/' + DEFAULT_LINUX_FOLDER_NAME; // TODO

        // The default directory 
        public static string DefaultInstallPath => IsOnWindows ? DefaultWindowsInstallPath : "";
        public static string DefaultFolderName => IsOnWindows ? DEFAULT_WINDOWS_FOLDER_NAME : "Pulsarc";

        // Default Temp Folder directory
        // GetTempPath should work on Mac and Linux as well.
        public static string DefaultTempPath => Path.GetTempPath().Replace('\\', '/') + "Pulsarc/";
        public static string DefaultTempFile => "PulsarcLauncher.txt";

        public static void CheckForAndExecuteNewInstall()
        {
            if (CanFindPulsarcDirectory())
                return;

            InstallTimer();
        }

        private static bool CanFindPulsarcDirectory()
        {
            string filePath = $"{DefaultTempPath}{DefaultTempFile}"; ;

            // If no temp file exists we can't know where the install directory is.
            if (!File.Exists(filePath))
                return false;

            // Read the first line
            string firstLine = "";
            using (var reader = new StreamReader(filePath)) { firstLine = reader.ReadLine(); }

            // First line should be "Dir: {InstallDirectory}"
            if (firstLine.Substring(0, 5) != "Dir: ")
                return false;

            // Get InstallPath
            string installPath = firstLine.Substring(5);

            // If the directory in the Temp file doesn't exist, install a new one.
            if (!Directory.Exists(installPath))
                return false;

            return true;
        }

        private static Task InstallTimer()
        {
            bool installing = false;
            string installPath = DefaultInstallPath;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            int lastTimeRemaining = 6;

            return Task.Factory.StartNew(async () =>
            {
                while (!installing)
                {
                    int timeRemaining = 5 - (int)Math.Floor(timer.ElapsedMilliseconds / 1000d);

                    if (timeRemaining != lastTimeRemaining)
                    {
                        lastTimeRemaining = timeRemaining;
                        PulsarcLauncherForm.ChangeText($"Installing to {installPath} in {timeRemaining} seconds.");
                    }

                    installing = timeRemaining <= 0;
                }
            });
        }

        public static void CheckForAndExecuteNewUpdate()
        {

        }
    }
}
