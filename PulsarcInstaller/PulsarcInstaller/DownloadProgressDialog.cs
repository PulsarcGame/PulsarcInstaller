using Eto.IO;
using Eto.Forms;
using Eto.Drawing;
using System;
using System.Net;
using System.ComponentModel;
using System.IO;
using PulsarcInstaller.Util;

namespace PulsarcInstaller
{
    public class DownloadProgressDialog : Dialog<DialogResult>
    {
        // Used to download data from server
        private WebClient webClient;

        private BackgroundWorker bgWorker;

        public string TempFilePath { get; private set; }

        private string md5;

        private ProgressBar downloadProgress = new ProgressBar()
        {
            MinValue = 0,
            MaxValue = 100,
            Value = 0,
            Size = new Size(350, 25),
        };

        private Label progressText = new Label()
        {
            TextAlignment = TextAlignment.Center,
        };

        public DownloadProgressDialog(Uri location, string md5)
        {
            InitializeComponent();

            // Create a temp path
            TempFilePath = Path.GetTempFileName();

            this.md5 = md5;

            SetUpClientAndWorker();

            try { using (webClient) { webClient.DownloadFileAsync(location, TempFilePath); } }
            catch { CloseWithResult(DialogResult.No); }
        }

        #region Init Methods
        private void InitializeComponent()
        {
            SetUpWindow();
            SetUpContent();
        }

        /// <summary>
        /// Prepares the window for displaying.
        /// </summary>
        private void SetUpWindow()
        {
            Size = new Size(400, 100);

            // Center the window
            Location = new Point(
                (int)(Screen.WorkingArea.Width - Size.Width) / 2,
                (int)(Screen.WorkingArea.Height - Size.Height) / 2);

            Icon = new Icon("Assets/Icon.ico");

            // Make window unmoving, unchanging
            Maximizable = false;
            Minimizable = false;
            Resizable = false;
            WindowStyle = WindowStyle.None;
        }

        /// <summary>
        /// Prepares the content to be displayed in the window.
        /// </summary>
        private void SetUpContent()
        {
            Content = new TableLayout
            {
                Rows =
                {
                    // Downloading
                    new Label()
                    {
                        Text = "Downloading...",
                        TextAlignment = TextAlignment.Center,
                    },
                    downloadProgress,
                    progressText,
                },

                Padding = new Padding(5),
                Spacing = new Size(10, 10),

                BackgroundColor = new Color(94 / 255f, 94 / 255f, 94 / 255f),
            };
        }

        private void SetUpClientAndWorker()
        {
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(WC_DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(WC_DownloadFileCompleted);

            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(BW_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW_RunWorkerCompleted);
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            bool triggeredAbort = false;

            // Stop webClient if it's busy
            if (webClient.IsBusy)
            {
                webClient.CancelAsync();
                triggeredAbort = true;
            }

            // Stop bgWorker if it's busy
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
                triggeredAbort = true;
            }

            if (triggeredAbort)
                Result = DialogResult.Abort;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (webClient.IsBusy)
                webClient.CancelAsync();
            webClient.Dispose();

            if (bgWorker.IsBusy)
                bgWorker.CancelAsync();
            bgWorker.Dispose();

            TempFilePath = null;
            md5 = null;

            downloadProgress.Dispose();
            progressText.Dispose();
        }

        #region WebClient and BackgroundWorker methods
        private void WC_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update progress bar
            downloadProgress.Value = e.ProgressPercentage;

            // Update progress text
            string bytesReceived = FormatBytes(e.BytesReceived);
            string totalBytes = FormatBytes(e.TotalBytesToReceive);
            progressText.Text = $"{bytesReceived} / {totalBytes}";

            // Formats bytes to be more readable.
            // i.e., we're downloading 3000 bytes, this method will change format from "x B / 3000 b"
            // to "x KB / 2.92 KB"
            string FormatBytes(in long bytes, int decimalPlaces = 2)
            {
                double byteAmount = bytes;
                string formatString = "{0";
                string byteType = "B";

                // Convert from B to KB, MB, or GB
                while (byteAmount > 1024 && byteType != "GB")
                    UpgradeType(ref byteAmount, ref byteType);

                // Format decimal places
                if (decimalPlaces > 0)
                    formatString += ":0.";
                for (int i = 0; i < decimalPlaces; i++)
                    formatString += "0";
                formatString += "} " + byteType;

                return string.Format(formatString, byteAmount);

                // Changes bytes to kilobytes, kilobytes to megabytes, and megabytes to gigabytes
                // As needed
                void UpgradeType(ref double amount, ref string currentByteType)
                {
                    amount /= 1024;

                    switch (currentByteType)
                    {
                        case "B":
                            currentByteType = "KB";
                            break;
                        case "KB":
                            currentByteType = "MB";
                            break;
                        case "MB":
                            currentByteType = "GB";
                            break;
                    }
                }
            }
        }

        private void WC_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
                CloseWithResult(DialogResult.No);
            else if (e.Cancelled)
                CloseWithResult(DialogResult.Abort);
            else
            {
                progressText.Text = "Verifying Download...";
                using (bgWorker) { bgWorker.RunWorkerAsync(new string[] { TempFilePath, md5 }); }
            }
        }

        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            // These arguments were given in WC_DownloadFileCompleted()
            string file = ((string[])e.Argument)[0];
            string updateMD5 = ((string[])e.Argument)[1];

            // Checksum with the MD5
            if (Hasher.HashFile(file, HashType.MD5) != updateMD5)
                e.Result = DialogResult.No;
            else
                e.Result = DialogResult.Ok;
        }

        /// <summary>
        /// When everything is done, close with the DialogResult received from the work
        /// </summary>
        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CloseWithResult((DialogResult)e.Result);
        }
        #endregion

        /// <summary>
        /// Closes this Dialog and makes sure the provided result is assigned before closing.
        /// </summary>
        private void CloseWithResult(DialogResult result)
        {
            Result = result;
            Close();
        }
    }
}
