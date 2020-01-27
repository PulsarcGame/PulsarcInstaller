using IWshRuntimeLibrary;

namespace PulsarcInstaller.Util
{
    public static class Shortcut
    {
        // Windows only
        public static void CreateOnWindows(string path)
        {
            object shDesktop = "Desktop";
            WshShell shell = new WshShell();
            
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop)
                + @"\Pulsarc.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = path + @"\Pulsarc.exe";
            shortcut.WorkingDirectory = path;
            shortcut.Save();
        }
    }
}
