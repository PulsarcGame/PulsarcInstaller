using Eto.Forms;
using PulsarcLauncher.Util;
using System.Threading;

namespace PulsarcLauncher
{
    public partial class PulsarcLauncherForm : Form
    {
        private static SynchronizationContext syncContext;
        public static SynchronizationContext SyncContext
        {
            get => syncContext;
            private set
            {
                if (syncContext != null)
                    return;

                syncContext = value;
            }
        }

        public PulsarcLauncherForm()
        {
            SyncContext = SynchronizationContext.Current;
            
            InitializeComponent();
        }
    }
}
