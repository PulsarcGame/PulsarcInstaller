using Eto.Forms;
using Eto.Drawing;
using PulsarcInstaller.Util;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PulsarcInstaller
{
    public class PulsarcInstallerForm : Form
    {
        // The label that gives information about installing/updating/launching to the user.
        private Label statusLabel = new Label()
        { TextAlignment = TextAlignment.Center, };

        private Stopwatch installTimer = new Stopwatch();

        // Keeps track of the installTimer, will also start and stop it automatically.
        // If you change this from false to true, the installTimer will restart.
        // If you change this from true to false, the installTimer will stop.
        private bool InstallTimerActive
        {
            get => installTimer.IsRunning;
            set
            {
                // Prevent extra Restart and Stop calls
                if (value == installTimer.IsRunning)
                    return;

                if (value)
                    installTimer.Restart();
                else
                    installTimer.Stop();
            }
        }

        // True if Pulsarc is already installed, and waiting from input from the player to
        // Change install location or close the Installer.
        private bool waitingForRightClick = false;

        // When this is true, the user can double click the form to change Install location
        private bool canAcceptRightClick => ComputerInfo.IsOnWindows && waitingForRightClick;

        // Defaults to the Default path first. Can be changed by the user later.
        private string installPath = ComputerInfo.DefaultInstallPath;

        /// <summary>
        /// Create a new PulsarcLauncherForm
        /// </summary>
        public PulsarcInstallerForm() => InitializeComponent();

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
            Size = new Size(300, 350);

            // Center the window
            Location = new Point(
                (int)(Screen.WorkingArea.Width - Size.Width) / 2,
                (int)(Screen.WorkingArea.Height - Size.Height) / 2);

            Icon = new Icon("assets/Icon.ico");

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
            // Place the logo and text into our window.
            Content = new TableLayout
            {
                Rows =
                {
                    // Logo
                    Bitmap.FromResource(Assets.Logo),
                    // Status Text
                    statusLabel,
                    // TODO: Version selector? Force launch/Force update? Button to Pulsarc folder directory?
                },

                Padding = new Padding(5),
                Spacing = new Size(30, 30),

                // TODO: Test the colors on other monitors/operating systems
                BackgroundColor = new Color(94 / 255f, 94 / 255f, 94 / 255f),
            };
        }
        #endregion

        #region Overriden Events
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Currently Installer only works for Windows
            if (ComputerInfo.IsOnWindows)
                StartInstallTimer();
            else
            {
                statusLabel.Text = "Sorry, but your platform is not supported yet." +
                    "Right click to close this window.";

                waitingForRightClick = true;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // Open Folder Select Dialog if the timer is active
            if (InstallTimerActive)
                ChooseInstallLocation();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // If the right button is pressed while waiting for input Message is up, close
            if (canAcceptRightClick && e.Buttons == MouseButtons.Alternate)
                Close();
        }
        #endregion

        #region Installation Methods
        /// <summary>
        /// Starts a timer that counts down from 10 seconds before installation.
        /// This gives the user a few seconds to decide a new install location.
        /// Uses async to update the UI and let the user know the time left.
        /// Similar to osu's installation method.
        /// </summary>
        private async void StartInstallTimer()
        {
            if (ComputerInfo.PulsarcDirectoryExistsIn(installPath))
            {
                ShowAlreadyInstalledMessage();
                return;
            }

            waitingForRightClick = false;

            Task InstallTimer = new Task(RunInstallTimer);
            InstallTimer.Start();

            // Wait for the timer to finish before installing Pulsarc
            await InstallTimer;

            statusLabel.Text = "Downloading...";

            // Install Pulsarc
            Installer installer = new Installer(installPath, this);
            installer.DoInstall();

            await Task.Run(() => WaitingForInstallation(ref installer));

            statusLabel.Text = "Installation is complete, launching Pulsarc...";
            // ... Launch Pulsarc ...

            Close();
        }

        private void WaitingForInstallation(ref Installer installer)
            { while (installer.InstallationComplete != true) { } }

        /// <summary>
        /// Opens a folder-select dialog to choose a new install location.
        /// </summary>
        private void ChooseInstallLocation()
        {
            // Stop timer
            InstallTimerActive = false;

            // Bring up dialog
            SelectFolderDialog selectFolder = new SelectFolderDialog();
            selectFolder.ShowDialog(this);

            // Use the selected directory as the new install path.
            // If the user cancelled out of the dialog, we get an error here,
            // in that case just catch it and continue.
            try { installPath = selectFolder.Directory; } catch { }

            // Reset timer and countdown again.
            InstallTimerActive = true;
        }
        
        /// <summary>
        /// Displays a message telling the user that Pulsarc is already installed
        /// at the selected directory.
        /// </summary>
        private void ShowAlreadyInstalledMessage()
        {
            waitingForRightClick = true;

            statusLabel.Text = "It seems that Pulsarc" +
                "\nis already installed at" +
                $"\n{installPath}" +
                $"\nRight Click to close this window," +
                $"\nOr double click to choose a new install location.";
        }

        /// <summary>
        /// Updates the text as the timer counts down each second.
        /// </summary>
        private void RunInstallTimer()
        {
            // Start timer
            InstallTimerActive = true;

            // Total length (in seconds) the timer should elapse for.
            const int TOTAL_TIME = 1;

            // Keep track of the previous second on the timer.
            int lastTimeRemaining = TOTAL_TIME + 1;

            while (true)
            {
                // Find remaining time by subtracting the current time passed (rounded down)
                // from the total time.
                int timeRemaining = TOTAL_TIME - 
                    (int)Math.Floor(installTimer.ElapsedMilliseconds / 1000d);

                // If a new second has passed
                if (timeRemaining != lastTimeRemaining)
                {
                    lastTimeRemaining = timeRemaining;

                    // Protection against Grammar Nazis
                    string plural = timeRemaining != 1 ? "s" : "";

                    // Create an action that changes the text to match timer...
                    Action lowerTimer = () =>
                        statusLabel.Text = $"Pulsarc will install at" +
                        $"\n{installPath}" +
                        $"\nin {timeRemaining} second{plural}." +
                        $"\nDouble click to change the install location.";

                    // ...And invoke that action since we aren't on the same thread as the UI.
                    Assets.Application.Invoke(lowerTimer);
                }

                // Stop the timer and this loop once we're at or below zero
                if (timeRemaining <= 0)
                {
                    InstallTimerActive = false;
                    return;
                }
            }
        }
        #endregion
    }
}