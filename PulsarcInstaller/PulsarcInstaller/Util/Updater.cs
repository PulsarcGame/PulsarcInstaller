using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PulsarcLauncher.Util
{
    // Saving this for AutoUpdate within Pulsarc.
    [Obsolete("This project is now for installing pulsarc, not installing & updating.")]
    class Updater
    {
        private BackgroundWorker bgWorker;

        public Updater()
        {
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
            throw new NotImplementedException();
        }

        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
