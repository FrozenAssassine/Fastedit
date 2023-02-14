using Fastedit.Tab;
using System;
using Windows.UI.Xaml;

namespace Fastedit.Helper
{
    internal class AutoDatabaseSaveHelper
    {
        public static TimeSpan DatabaseBackupTime = new TimeSpan(0, 4, 0);

        public static void RegisterSave()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = DatabaseBackupTime;
            timer.Tick += SaveDatabaseTimer_Tick;
            timer.Start();
        }

        private static async void SaveDatabaseTimer_Tick(object sender, object e)
        {
            await TabPageHelper.mainPage.SaveDatabase(false, false);
        }
    }
}
