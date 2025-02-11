using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Fastedit.Views;

namespace Fastedit.Dialogs
{
    public class RecycleBinDialog
    {
        private TabView tabView = null;
        private ContentDialog dialog;

        public async Task ShowAsync(TabView tabView)
        {
            this.tabView = tabView;
            this.dialog = new ContentDialog()
            {
                Background = DialogHelper.ContentDialogBackground(),
                Foreground = DialogHelper.ContentDialogForeground(),
                RequestedTheme = DialogHelper.DialogDesign,
                Title = "Recycle bin",
                CloseButtonText = "Close",
                Content = new RecycleBinDialogPage(tabView),
                XamlRoot = App.m_window.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}