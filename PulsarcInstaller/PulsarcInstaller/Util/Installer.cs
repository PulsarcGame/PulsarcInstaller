using Eto.Forms;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace PulsarcInstaller.Util
{
    public class Installer
    {
        // Checks if CurrentVersion.xml exists and starts the download process.
        private BackgroundWorker bgWorker;

        // The path to install Pulsarc in.
        private string installPath;

        // Parent control
        private Control parentControl;

        public bool InstallationComplete { get; private set; } = false;

        public Installer(string installPath, Control control)
        {
            parentControl = control;

            this.installPath = installPath;

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(BW_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW_RunWorkerCompleted);
        }

        /// <summary>
        /// The only method other classes should access. Starts the download process then installs
        /// Pulsarc.
        /// </summary>
        public void DoInstall()
        {
            if (!bgWorker.IsBusy)
                bgWorker.RunWorkerAsync();
        }

        #region BackgroundWorker Methods
        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            Uri uri = new Uri(ComputerInfo.ServerDownloadPath);

            // Check if CurrentVersion.xml exists on the Pulsarc server.
            // Cancel if it doesn't exist. Continue with the process otherwise.
            if (!InstallXML.Exists(uri))
                e.Cancel = true;
            else
                e.Result = InstallXML.Parse(uri);
        }

        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                // Check to make sure the xml file is fine. If it is, continue on to the Download.
                InstallXML install = (InstallXML)e.Result;
                if (install != null)
                    DownloadAndInstall(install);
            }
        }
        #endregion

        #region Install Methods
        private void DownloadAndInstall(InstallXML install)
        {
            // Download Pulsarc
            DialogResult result = DialogResult.None;
            string tempFilePath = "";

            using (var progressForm = new DownloadProgressDialog(install.DownloadUri, install.MD5))
            {
                tempFilePath = progressForm.TempFilePath;
                result = progressForm.ShowModal(parentControl);
            }

            // If download was successful, Install Pulsarc
            if (result == DialogResult.Ok)
                InstallPulsarc(tempFilePath, installPath);

            // If the download was aborted, let the user know.
            else if (result == DialogResult.Abort)
                MessageBox.Show("The download was cancelled",
                    "Download Cancelled",
                    MessageBoxButtons.OK,
                    MessageBoxType.Information);

            // If an error happened during downloading, let the user know.
            else
                MessageBox.Show("There was a problem during downloading.\nPlease try again.",
                    "Download Error",
                    MessageBoxButtons.OK,
                    MessageBoxType.Information);
        }

        private void InstallPulsarc(string tempFilePath, string installPath, string launchArgs = "")
        {
            // If the provided installation directory does not contain "Pulsarc", then add it
            // Safegaurd for users who may just try to install onto their Desktop or similar.
            if (!installPath.ToLower().Contains("pulsarc"))
                installPath += "Pulsarc/";

            if (!File.Exists(tempFilePath))
                return;

            string zipFilePath = Path.ChangeExtension(tempFilePath, ".zip");

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    File.Copy(tempFilePath, zipFilePath);
                    break;
                }
                catch
                {
                    if (i <= 4)
                    {
                        MessageBox.Show("There was a problem during installation.\nPlease try again.",
                            "Installation Error",
                            MessageBoxButtons.OK,
                            MessageBoxType.Information);

                        goto DeleteFiles;
                    }
                }
            }

            using (FileStream input = File.OpenRead(zipFilePath))
            using (ZipFile zipFile = new ZipFile(input))
            {
                foreach (ZipEntry entry in zipFile)
                {
                    // Ignore uneeded entries
                    if (!KeepEntry(entry))
                        continue;

                    // entry.Name includes the full path relative to the .zip file
                    // Add that path ontop of our installPath to move the files accordingly.
                    string newPath = Path.Combine(installPath, entry.Name);

                    // Create a new directory if needed.
                    string directoryName = Path.GetDirectoryName(newPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // According to SharpZipLib 4KB is optimum
                    byte[] buffer = new byte[4096];

                    // Unzip the file in buffered chunks.
                    // According to SharpZipLib this uses less memory than unzipping the whole
                    // thing At once.
                    using (Stream zipStream = zipFile.GetInputStream(entry))
                    using (Stream output = File.Create(newPath))
                    {
                        StreamUtils.Copy(zipStream, output, buffer);

                        if (ShouldHideFile(entry.Name))
                            File.SetAttributes(newPath, File.GetAttributes(newPath) | FileAttributes.Hidden);
                    }
                }
            }

            // Create Desktop Shortcut
            if (ComputerInfo.IsOnWindows)
                Shortcut.CreateOnWindows(installPath);
            // TODO: Linux/Max desktop shortcuts.

            DeleteFiles:
            Thread.Sleep(2000);
            try { File.Delete(zipFilePath); } catch { try { File.Delete(zipFilePath); } catch { } }
            try { File.Delete(tempFilePath); } catch { try { File.Delete(tempFilePath); } catch { } }

            InstallationComplete = true;

            // Whether or not an entry is needed for this installation.
            // TODO: Clean the .zip files before uploading rather than having the installer do it.
            bool KeepEntry(in ZipEntry entry)
            {
                // If the entry is a file without the .pdb extension and is good for the
                // current platform, return true.
                return entry.IsFile && !entry.Name.Contains(".pdb") && ForRightPlatform(entry.Name);

                bool ForRightPlatform(in string name)
                {
                    if (ComputerInfo.IsOnWindows)
                    {
                        /*// If 64 bit, don't care about 32 bit dlls
                        if (ComputerInfo.Is64Bit && name.Contains("x64"))
                            return false;

                        // If 32 bit, don't care about 64 bit dlls
                        if (!ComputerInfo.Is64Bit && name.Contains("x86"))
                            return false;*/

                        // Get rid of .dylibs and .sos we don't need them.
                        if (name.Contains(".dylib") || name.Contains(".so"))
                            return false;
                    }
                    else if (ComputerInfo.IsOnMac)
                    {
                        // Mac doesn't use .dlls
                        // Might not use .sos either
                        if (name.Contains(".dll"))
                            return false;
                    }
                    else if (ComputerInfo.IsOnLinux)
                    {
                        // Linux doesn't use .dlls or .dylibs
                        if (name.Contains(".dll") || name.Contains(".dylib"))
                            return false;
                    }

                    return true;
                }
            }

            bool ShouldHideFile(in string name)
            {
                string[] hiddenFileTypes = { ".dll", ".so", ".dylib", ".config", ".json", };

                foreach (string fileType in hiddenFileTypes)
                    if (name.Contains(fileType))
                        return true;

                return false;
            }
        }
        #endregion
    }
}
