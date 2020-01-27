using Eto.Forms;

namespace PulsarcInstaller.Util
{
    public static class Assets
    {
        // Eto uses assembly format for image resources, this is the prefix for the assets
        public static readonly string AssetAssembly = "PulsarcInstaller.assets.";

        public static readonly string IconPNG = $"{AssetAssembly}icon.png";
        public static readonly string Logo = $"{AssetAssembly}logo.png";

        // The application. Used for Invoke() and other method we can't access otherwise.
        public static Application Application = new Application(Eto.Platform.Detect);
    }
}
