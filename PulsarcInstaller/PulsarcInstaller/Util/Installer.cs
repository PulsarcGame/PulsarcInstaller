using Eto.Forms;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            DownloadProgressDialog progressForm = new DownloadProgressDialog(install.DownloadUri, install.MD5);
            DialogResult result = progressForm.ShowModal(parentControl);

            // If download was successful, Install Pulsarc
            if (result == DialogResult.Ok)
                InstallPulsarc(progressForm.TempFilePath, installPath);

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
            if (installPath.ToLower().Contains("pulsarc"))
                installPath += "Pulsarc/";

            using (Stream input = File.OpenRead(tempFilePath))
            using (ZipFile zipFile = new ZipFile(input))
            {
                foreach (ZipEntry entry in zipFile)
                {
                    // Ignore Directories and debug files
                    if (!entry.IsFile || entry.Name.Contains(".pdb"))
                        continue;

                    // If the file is a .dll prepare for it for moving into "lib" folder.
                    // Pulsarc is set upt to look in the lib folder for .dlls
                    if (entry.Name.Contains(".dll") || entry.Name.Contains(".dylib"))
                        installPath += "lib/";

                    // entry.Name should include the full path RELATIVE TO THE ZIP FILE
                    string newPath = Path.Combine(installPath, entry.Name);
                    string directoryName = Path.GetDirectoryName(newPath);

                    // Create a new directory if needed.
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // According to SharpZipLib 4k is optimum
                    byte[] buffer = new byte[4096];

                    // Unzip the file in buffered chunks.
                    // According to SharpZipLib this uses less memory than unzipping the whole
                    // thing.
                    using (Stream zipStream = zipFile.GetInputStream(entry))
                    using (Stream output = File.Create(newPath))
                        StreamUtils.Copy(zipStream, output, buffer);
                }
            }
        }
        #endregion
    }
}
