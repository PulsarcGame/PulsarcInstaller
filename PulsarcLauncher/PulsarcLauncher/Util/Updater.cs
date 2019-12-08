using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Reflection;

namespace PulsarcLauncher.Util
{
    public static class Updater
    {
        // Windows
        public static bool IsOnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool Is64Bit => Environment.Is64BitProcess;

        // %appdata% folder
        private const string DEFAULT_WINDOWS_FOLDER_NAME = ".Pulsarc";
        private static readonly string DefaultWindowsInstallPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace('\\', '/')
            + '/' + DEFAULT_WINDOWS_FOLDER_NAME;

        // Mac / OSX
        public static bool IsOnMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
        // TODO
        private const string DEFAULT_MAC_FOLDER_NAME = "Pulsarc";
        private static string DefaultMacInstallPath = "" + '/' + DEFAULT_MAC_FOLDER_NAME;

        // Linux
        public static bool IsOnLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
        // TODO
        private const string DEFAULT_LINUX_FOLDER_NAME = "Pulsarc";
        private static string DefaultLinuxInstallPath = "" + '/' + DEFAULT_LINUX_FOLDER_NAME;

        // The default directory 
        public static string DefaultInstallPath => IsOnWindows ? DefaultWindowsInstallPath : "";
        public static string DefaultFolderName => IsOnWindows ? DEFAULT_WINDOWS_FOLDER_NAME : "Pulsarc";

        /// <summary>
        /// Determine whether or not the Pulsarc Directory Exists.
        /// </summary>
        /// <returns>True if Pulsarc has been installed and there is a directory.
        /// or False if it can't be found.</returns>
        public static bool PulsarcDirectoryExists()
        {
            string assemblyPath = GetPathToAssembly();

            // If the path we're in has only one or two "/" it means we're definitely not in the Pulsarc directory.
            // We're in "[X]:/.../Pulsarc/[.dllFolder], at least 3 "/"
            if (assemblyPath.Count(c => c == '/') <= 2)
                return false;

            // If we don't see Pulsarc.exe in the folder above us, we're probably not in the right place.
            int indexOfLastSlash = assemblyPath.LastIndexOf('/');
            string rootAppFolder = assemblyPath.Substring(0, indexOfLastSlash);

            return File.Exists($"{rootAppFolder}/Pulsarc.exe");
        }

        /// <summary>
        /// Get the path to the currently executing assembly (.dll) file.
        /// </summary>
        /// <returns></returns>
        private static string GetPathToAssembly()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                // Change windows "\" to "/" for easier usage.
                .Replace("\\", "/");
        }

        public static void UpdateIfNeeded()
        {

        }
    }
}
