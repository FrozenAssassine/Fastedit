using System;
using Microsoft.UI.Xaml;
using Fastedit.Core.Tab;

namespace Fastedit.Helper;

internal class AutoSaveDatabaseHelper
{
    public static TimeSpan DatabaseBackupTime = new TimeSpan(0, 4, 0);

    public static void RegisterSave()
    {
        DispatcherTimer timer = new DispatcherTimer();
        timer.Interval = DatabaseBackupTime;
        timer.Tick += SaveDatabaseTimer_Tick;
        timer.Start();
    }

    private static void SaveDatabaseTimer_Tick(object sender, object e)
    {
        TabPageHelper.mainPage.SaveDatabase(false, false);
    }
}
