using Eto.Forms;
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
        private const string CURRENT_VERSION_PATH = "https://pulsarc.net/Releases/";

        private BackgroundWorker bgWorker;

        private string installPath;

        public Installer(string installpath)
        {
            this.installPath = installPath;

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(BW_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW_RunWorkerCompleted);
        }

        public void DoUpdate()
        {
            if (!bgWorker.IsBusy)
                bgWorker.RunWorkerAsync();
        }

        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            Uri uri = new Uri(CURRENT_VERSION_PATH);

            if (!InstallXML.Exists(uri))
                e.Cancel = true;
            else
                e.Result = InstallXML.Parse(uri);
        }

        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                InstallXML install = (InstallXML)e.Result;

                if (install != null)
                    DownloadAndInstall(install);
            }
        }

        private void DownloadAndInstall(InstallXML install)
        {
            // Download Pulsarc
            DownloadProgressForm progressForm = new DownloadProgressForm(install.DownloadUri, install.MD5);
            DialogResult result = progressForm.Show();

            // If download was successful, Install Pulsarc
            if (result == DialogResult.Ok)
                InstallPulsarc(progressForm.TempFilePath, installPath);
            else if (result == DialogResult.Abort)
                MessageBox.Show("The download was cancelled", "Download Cancelled", MessageBoxButtons.OK, MessageBoxType.Information);
            else
                MessageBox.Show("There was a problem during downloading.\nPlease try again.", "Download Error", MessageBoxButtons.OK, MessageBoxType.Information);
        }

        private void InstallPulsarc(string tempFilePath, string installPath, string launchArgs = "")
        {
            // Borrowed from https://youtu.be/Oa7vMrGKifo (end of video)

            // TODO: Change this method so it can work on all platforms (don't rely on cmd for moving files)
            //       Figure out how to unarchive a file, as the download will be a .zip.

            int timeOutTime = 4;

            string argument = 
                // Timeout for x seconds
                $"/C Choice /C Y /N /D Y /T {timeOutTime}" +
                // Move contents of tempPath to installPath
                $"Move /Y \"{tempFilePath}\" \"{installPath}\"" +
                // Start the app?
                $"& Start \"\" /D \"{Path.GetDirectoryName(installPath)}\" \"{Path.GetFileName(installPath)}\" {launchArgs}";

            ProcessStartInfo info = new ProcessStartInfo()
            {
                Arguments = argument,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
            };

            Process.Start(info);
        }
    }
}
