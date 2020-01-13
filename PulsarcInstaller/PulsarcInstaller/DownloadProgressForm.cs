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
    public class DownloadProgressForm : Form
    {
        // Used to download data from server
        private WebClient webClient;

        private BackgroundWorker bgWorker;

        public string TempFilePath { get; private set; }

        private string md5;

        private bool full;

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

        public DownloadProgressForm(Uri location, string md5, bool install)
        {
            InitializeComponent();

            // Create a temp path
            TempFilePath = Path.GetTempFileName();

            this.md5 = md5;

            SetUpClientAndWorker();

            try { webClient.DownloadFileAsync(location, TempFilePath); }
            catch { CloseWithDialog(DialogResult.No); }
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
            Size = new Size(400, 200);

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
            bgWorker.DoWork += new DoWorkEventHandler(BW_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW_RunWorkerCompleted);
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            bool triggeredDialog = false;

            // Stop webClient if it's busy
            if (webClient.IsBusy)
            {
                webClient.CancelAsync();
                triggeredDialog = true;
            }

            // Stop bgWorker if it's busy
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
                triggeredDialog = true;
            }

            // Close with an Abort result
            if (triggeredDialog)
                CloseWithDialog(DialogResult.Abort);
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
            // i.e., we're downloading 3000 bytes, the format will change from "x B / 3000 b" to
            // "x KB / 2.92 KB"
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
                CloseWithDialog(DialogResult.No);
            else if (e.Cancelled)
                CloseWithDialog(DialogResult.Abort);
            else
            {
                progressText.Text = "Verifying Download...";
                bgWorker.RunWorkerAsync(new string[] { TempFilePath, md5 });
            }
        }

        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            // These arguments were given in WC_DownloadFileCompleted()
            string file = ((string[])e.Argument)[0];
            string updateMD5 = ((string[])e.Argument)[1];

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
            CloseWithDialog((DialogResult)e.Result);
        }
        #endregion

        // Not sure how to set the DialogResult for this form using Eto,
        // Pretty sure this just pops up a dialog window with the DialogResult provided
        // And then closes the whole thing when it's done, but haven't tested yet.
        // TODO?: Turn this class from a "Form" into a "Dialog" instead?
        private void CloseWithDialog(DialogResult result)
        {
            Dialog<DialogResult> dialog = new Dialog<DialogResult>()
                { Result = result, };

            dialog.ShowModal(this);
            dialog.Close(result);

            Close();
        }
    }
}
