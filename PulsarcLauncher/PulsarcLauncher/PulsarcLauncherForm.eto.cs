using Eto.Forms;
using Eto.Drawing;
using PulsarcLauncher.Util;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PulsarcLauncher
{
    partial class PulsarcLauncherForm : Form
    {
        // The label that gives information about installing/updating/launching to the user.
        private Label StatusLabel;

        private Stopwatch InstallTimer = new Stopwatch();
        
        // Keeps track of the InstallTimer, will also start and stop it automatically.
        private bool InstallTimerActive
        {
            get => InstallTimer.IsRunning;
            set
            {
                // Prevent extra Restart and Stop calls
                if (value == InstallTimer.IsRunning)
                    return;

                // If true, restart the timer
                if (value)
                    InstallTimer.Restart();
                // If false, stop the timer
                else
                    InstallTimer.Stop();
            }
        }

        private string InstallPath = Updater.DefaultInstallPath;

        private void InitializeComponent()
        {
            SetUpWindow();

            SetUpContent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // ... TODO?: Check for launcher-specific updates ...

            if (!Updater.PulsarcDirectoryExists())
                StartInstallTimer();
            else
                Updater.UpdateIfNeeded();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // Open Folder Dialogue if the timer is active
            if (InstallTimerActive)
                ChooseInstallLocation();
        }

        private void ChooseInstallLocation()
        {
            InstallTimerActive = false;

            SelectFolderDialog selectFolder = new SelectFolderDialog();
            selectFolder.ShowDialog(this);

            // If the user cancelled out of the dialog, we get an error here,
            // in that case just catch it and continue.
            try { InstallPath = selectFolder.Directory; } catch { }

            InstallTimerActive = true;
        }

        /// <summary>
        /// Starts a timer that counts down from 10 seconds before installation.
        /// Uses async to update the UI and let the user know the time left.
        /// Similar to osu's installation method.
        /// </summary>
        private async void StartInstallTimer()
        {
            Task InstallTimer = new Task(RunInstallTimer);
            InstallTimer.Start();
            await InstallTimer;
        }

        /// <summary>
        /// Updates the text as the timer counts down each second.
        /// </summary>
        private void RunInstallTimer()
        {
            InstallTimerActive = true;

            // Total length (in seconds) the timer should elapse for.
            int totalTime = 10;
            // Keeps track of the last second that's passed on the timer.
            int lastTimeRemaining = totalTime + 1;

            while (true)
            {
                // Find remaining time by subtracting the current time passed (rounded down)
                // from the total time.
                int timeRemaining = totalTime
                    - (int)Math.Floor(InstallTimer.ElapsedMilliseconds / 1000d);

                // If a new second has passed
                if (timeRemaining != lastTimeRemaining)
                {
                    lastTimeRemaining = timeRemaining;

                    // Protection against Grammar Nazis
                    string plural = timeRemaining != 1 ? "s" : "";

                    // Create an action that changes the text to match timer.
                    Action lowerTimer = () =>
                        StatusLabel.Text = $"Pulsarc will install at" +
                        $"\n{InstallPath}" +
                        $"\nin {timeRemaining} second{plural}." +
                        $"\nDouble click to change the install location.";

                    // And invoke it since we aren't on the same thread as the UI.
                    Assets.Application.Invoke(lowerTimer);
                }

                // Stop the timer once we're below zero
                if (timeRemaining <= 0)
                {
                    InstallTimerActive = false;
                    return;
                }
            }
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
            StatusLabel = new Label()
            {
                Text = "Checking for updates...",
                TextAlignment = TextAlignment.Center,
            };

            // Place the logo and text into our window.
            Content = new TableLayout
            {
                Rows =
                {
                    // Logo
                    Bitmap.FromResource(Assets.Logo),
                    // Status Text
                    StatusLabel,
                    // TODO: Version selector? Force launch/Force update? Button to Pulsarc folder directory?
                },

                Padding = new Padding(5),
                Spacing = new Size(30, 30),

                // TODO: Test the colors on other computers/platforms
                BackgroundColor = new Color(94 / 255f, 94 / 255f, 94 / 255f),
            };
        }
    }
}
