using Fastedit.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;

namespace Fastedit.Controls
{
    public static class InfobarMessage
    {
        public static void Show(this InfoBar infobar, string title, string message, InfoBarSeverity severity, int showSeconds = 8)
        {
            Show(infobar, title, message, null, severity, showSeconds);
        }
        public static void Show(this InfoBar infobar, string title, string message, ButtonBase actionButton, InfoBarSeverity severity, int showSeconds = 5)
        {
            infobar.Title = title;
            infobar.Message = message;
            infobar.ActionButton = actionButton;
            infobar.Severity = severity;
            infobar.IsOpen = true;
            infobar.MaxWidth = 500;
            infobar.RequestedTheme = DialogHelper.DialogDesign;

            MainWindow.InfoMessagesPanel.Children.Add(infobar);

            DispatcherTimer autoCloseTimer = new DispatcherTimer();
            autoCloseTimer.Interval = new TimeSpan(0, 0, showSeconds);
            autoCloseTimer.Start();
            autoCloseTimer.Tick += delegate
            {
                infobar.IsOpen = false;
                autoCloseTimer.Stop();
            };
        }
    }
}
