using Eto.Forms;
using Eto.Drawing;
using PulsarcLauncher.Util;
using System;
using System.Threading;

namespace PulsarcLauncher
{
    partial class PulsarcLauncherForm : Form
    {
        // The label that gives information about installing/updating/launching to the user.
        private static Label StatusLabel;

        void InitializeComponent()
        {
            SetUpWindow();

            SetUpContent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Updater.CheckForAndExecuteNewInstall();

            Updater.CheckForAndExecuteNewUpdate();
        }

        void SetUpWindow()
        {
            // Set Size of the window to 300 x 350
            Size = new Size(300, 350);

            // Center the window
            Location = new Point((int)(Screen.WorkingArea.Width - Size.Width) / 2, (int)(Screen.WorkingArea.Height - Size.Height) / 2);

            // Set Icon
            Icon = new Icon(Assets.Icon);
            
            // Make window unchangeable
            Maximizable = false;
            Minimizable = false;
            Resizable = false;
            WindowStyle = WindowStyle.None;
        }

        void SetUpContent()
        {
            StatusLabel = new Label()
            {
                Text = "Checking for updates...",
                TextAlignment = TextAlignment.Center,
            };

            Content = new TableLayout
            {
                Rows =
                {
                    // Logo
                    Bitmap.FromResource(Assets.Logo),
                    // Status Text
                    StatusLabel,
                    // TODO: Version selector? Force launch/Force update. Button to Pulsarc folder directory.
                },

                Padding = new Padding(5),
                Spacing = new Size(30, 30),
            };
        }

        public static void ChangeText(string text)
        {
            SyncContext.Post(new SendOrPostCallback(o =>
            {
                if (StatusLabel == null)
                    return;

                Console.WriteLine(text);

                StatusLabel.Text = text;
            }), text);
        }
    }
}
