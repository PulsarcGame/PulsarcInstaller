using System;
using Eto.Forms;
using Eto.Drawing;
using PulsarcLauncher.Util;

namespace PulsarcLauncher
{
    partial class PulsarcLauncherForm : Form
    {
        void InitializeComponent()
        {
            Icon = new Icon(Assets.Icon);

            BuildLayout();
        }

        void BuildLayout()
        {
            Content = new TableLayout
            {
                Rows =
                {
                    Bitmap.FromResource(Assets.Logo).WithSize(500, 500),
                    new Label()
                    {
                        Text = "Welcome to the Pulsarc Launcher!",
                        TextAlignment = TextAlignment.Center,
                    },
                    null,
                    null
                },

                Padding = new Padding(5),
                Spacing = new Size(10, 10),
            };
        }
    }
}
