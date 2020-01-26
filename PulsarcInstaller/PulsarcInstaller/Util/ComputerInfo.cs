using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Reflection;

namespace PulsarcInstaller.Util
{
    public static class ComputerInfo
    {
        // Windows
        public static bool IsOnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool Is64Bit => Environment.Is64BitProcess;

        private const string WINDOWS_FILE_EXTENSION = ".exe";

        // %appdata% folder
        private const string DEFAULT_WINDOWS_FOLDER_NAME = ".Pulsarc/";
        private static readonly string DefaultWindowsInstallPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace('\\', '/')
            + '/' + DEFAULT_WINDOWS_FOLDER_NAME;

        // Mac / OSX
        public static bool IsOnMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        private const string MAC_FILE_EXTENSION = "";

        // TODO
        private const string DEFAULT_MAC_FOLDER_NAME = "Pulsarc/";
        private static string DefaultMacInstallPath = "TODO" + '/' + DEFAULT_MAC_FOLDER_NAME;

        // Linux
        public static bool IsOnLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        // TODO
        private const string DEFAULT_LINUX_FOLDER_NAME = "Pulsarc/";
        private static string DefaultLinuxInstallPath = "TODO" + '/' + DEFAULT_LINUX_FOLDER_NAME;
        private const string LINUX_FILE_EXTENSION = "TODO";

        // Invalid Platform Exception
        private static Exception invalidPlat = new Exception("Invalid Platform");

        // The default path
        public static string DefaultFolderName =>    IsOnWindows ? DEFAULT_WINDOWS_FOLDER_NAME
                                                   : IsOnMac ? DEFAULT_MAC_FOLDER_NAME
                                                   : IsOnLinux ? DefaultLinuxInstallPath
                                                   : throw invalidPlat;

        public static string DefaultInstallPath =>   IsOnWindows ? DefaultWindowsInstallPath
                                                   : IsOnMac ? DefaultMacInstallPath
                                                   : IsOnLinux ? DefaultLinuxInstallPath
                                                   : throw invalidPlat;

        public static string ExecutableExtension =>  IsOnWindows ? WINDOWS_FILE_EXTENSION
                                                   : IsOnMac ? MAC_FILE_EXTENSION
                                                   : IsOnLinux ? LINUX_FILE_EXTENSION
                                                   : throw invalidPlat;

        // The address to the xml file for installation
        private const string INSTALL_PATH_PREFIX = "https://pulsarc.net/Releases/CurrentVersion-";

        // The full path to the xml file for installation. Formatted as so:
        // "https://pulsarc.net/Releases/CurrentVersion-{platform}.xml"
        public static string ServerDownloadPath => INSTALL_PATH_PREFIX + "-" + (
                                                   IsOnWindows ?
                                                       Is64Bit ? "win64" : "win32"
                                                 : IsOnMac ? "osx"
                                                 : IsOnLinux ? "linux"
                                                 : throw invalidPlat
                                                 ) + ".xml";

        /// <summary>
        /// Sees if a Pulsarc executable is in the path provided.
        /// </summary>
        /// <param name="pathToCheck">The path to check for Pulsarc.</param>
        /// <returns></returns>
        public static bool PulsarcDirectoryExistsIn(string pathToCheck)
        {
            return File.Exists($"{pathToCheck}/Pulsarc{ExecutableExtension}");
        }

        /// <summary>
        /// Determine whether or not the Pulsarc Directory Exists.
        /// This method assumes the Launcher/Installer executable is in the same location
        /// as the Pulsarc executable.
        /// </summary>
        /// <returns>True if Pulsarc has been installed and there is a directory.
        /// or False if it can't be found.</returns>
        public static bool PulsarcDirectoryExists()
        {
            string assemblyPath = GetPathToAssembly();

            // If the path we're in has only one or two "/" it means we're definitely not in the Pulsarc directory.
            // Windows: "[X]:/.../Pulsarc/[.dllFolder]
            // Mac: "/.../Pulsarc/[.dllFolder]
            // Linux: TOFO (To figure out)
            if (assemblyPath.Count(c => c == '/') <= 2)
                return false;

            // If we don't see the Pulsarc executable in the folder above us, we're probably not in the right place.
            int indexOfLastSlash = assemblyPath.LastIndexOf('/');
            string rootAppFolder = assemblyPath.Substring(0, indexOfLastSlash);

            return PulsarcDirectoryExistsIn(rootAppFolder);
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
    }
}
