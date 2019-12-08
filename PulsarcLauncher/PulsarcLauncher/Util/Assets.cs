using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Text;

namespace PulsarcLauncher.Util
{
    public static class Assets
    {
        // Eto uses assembly format for image resources
        public static readonly string AssetAssembly = "PulsarcLauncher.assets.";

        // "PulsarcLauncher.assets.icon.png"
        public static readonly string IconPNG = $"{AssetAssembly}icon.png";

        // "PulsarcLauncher.assets.logo.png"
        public static readonly string Logo = $"{AssetAssembly}logo.png";

        // The whole application. Used for Invoke() and other method we can't access otherwise.
        public static Application Application = new Application(Eto.Platform.Detect);
    }
}
