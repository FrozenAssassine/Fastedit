using Fastedit.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;

namespace Fastedit.Controls
{
    public class InfobarMessage : InfoBar
    {
        public InfobarMessage(string title, string message, InfoBarSeverity severity, int showSeconds = 5)
        {
            Show(title, message, null, severity, showSeconds);
        }
        public InfobarMessage(string title, string message, UIElement content, InfoBarSeverity severity, int showSeconds = 5)
        {
            Show(title, message, content, severity, showSeconds);
        }

        private void Show(string title, string message, UIElement content, InfoBarSeverity severity, int showSeconds = 5)
        {
            this.Title = title;
            this.Message = message;
            this.Content = content;
            this.Severity = severity;
            this.IsOpen = true;
            //this.Background = DialogHelper.ContentDialogBackground();
            //this.Foreground = DialogHelper.ContentDialogForeground();
            this.RequestedTheme = DialogHelper.DialogDesign;

            DispatcherTimer autoCloseTimer = new DispatcherTimer();
            autoCloseTimer.Interval = new TimeSpan(0, 0, showSeconds);
            autoCloseTimer.Start();
            autoCloseTimer.Tick += delegate
            {
                this.IsOpen = false;
                autoCloseTimer.Stop();
            };
        }
    }
}
