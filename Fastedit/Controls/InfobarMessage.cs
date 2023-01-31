using Fastedit.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Fastedit.Controls
{
    public static class InfobarMessage
    {
        public static void Show(this InfoBar infobar, string title, string message, InfoBarSeverity severity, int showSeconds = 5)
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
            //this.Background = DialogHelper.ContentDialogBackground();
            //this.Foreground = DialogHelper.ContentDialogForeground();
            infobar.RequestedTheme = DialogHelper.DialogDesign;

            InfoMessages.InfoMessagePanel.Children.Add(infobar);

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
