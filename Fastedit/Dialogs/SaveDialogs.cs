using Fastedit.Controls.Textbox;
using Fastedit.Core;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public class SaveDialogs
    {
        public static async Task<ContentDialogResult> AskSaveDialogNeverSaved(muxc.TabViewItem TabPage)
        {
            TextControlBox textbox = (TextControlBox)TabPage.Content;
            AppSettings appsettings = new AppSettings();

            var SaveDialog = new ContentDialog
            {
                Background = DefaultValues.ContentDialogBackgroundColor(),
                Foreground = DefaultValues.ContentDialogForegroundColor(),
                CornerRadius = DefaultValues.DefaultDialogCornerRadius,
                Title = appsettings.GetResourceString("AskSaveDialog_Save/Text"),
                Content = appsettings.GetResourceString("AskSaveDialog_AskToSaveFile/Text") + "\n" + textbox.Header,
                PrimaryButtonText = appsettings.GetResourceString("AskSaveDialog_SaveButton/Text"),
                SecondaryButtonText = appsettings.GetResourceString("AskSaveDialog_DontSaveButton/Text"),
                CloseButtonText = appsettings.GetResourceString("AskSaveDialog_CancelButton/Text"),
                RequestedTheme = DefaultValues.ContentDialogTheme()
            };
            return await SaveDialog.ShowAsync();
        }
        public static async Task<ContentDialogResult> AskSaveDialog(muxc.TabViewItem TabPage)
        {
            TextControlBox textbox = (TextControlBox)TabPage.Content;

            AppSettings appsettings = new AppSettings();
            var SaveDialog = new ContentDialog
            {
                Background = DefaultValues.ContentDialogBackgroundColor(),

                CornerRadius = DefaultValues.DefaultDialogCornerRadius,
                Title = appsettings.GetResourceString("AskSaveDialog_Save/Text"),
                Content = appsettings.GetResourceString("AskSaveDialog_AskSaveContent/Text") + "\n" + textbox.Header.Replace("*", ""),
                PrimaryButtonText = appsettings.GetResourceString("AskSaveDialog_SaveButton/Text"),
                SecondaryButtonText = appsettings.GetResourceString("AskSaveDialog_DontSaveButton/Text"),
                CloseButtonText = appsettings.GetResourceString("AskSaveDialog_CancelButton/Text"),
                RequestedTheme = DefaultValues.ContentDialogTheme(),
                Foreground = DefaultValues.ContentDialogForegroundColor()
            };
            return await SaveDialog.ShowAsync();
        }
        public static async Task<ContentDialogResult> FileWasDeletedDialog(muxc.TabViewItem TabPage, bool ShowWithSecondaryButton = true)
        {
            TextControlBox textbox = (TextControlBox)TabPage.Content;
            AppSettings appsettings = new AppSettings();

            var SaveDialog = new ContentDialog
            {
                Background = DefaultValues.ContentDialogBackgroundColor(),
                Foreground = DefaultValues.ContentDialogForegroundColor(),
                CornerRadius = DefaultValues.DefaultDialogCornerRadius,
                Title = appsettings.GetResourceString("AskSaveDialog_Save/Text"),
                Content = appsettings.GetResourceString("AskSaveDialog_FileWasDeletedFromYourPc/Text") + "\n" + textbox.Header.Replace("*", ""),
                PrimaryButtonText = appsettings.GetResourceString("AskSaveDialog_SaveButton/Text"),
                CloseButtonText = appsettings.GetResourceString("AskSaveDialog_CancelButton/Text"),
                RequestedTheme = DefaultValues.ContentDialogTheme()
            };
            //Show second button only when tab gets closed
            if (ShowWithSecondaryButton)
            {
                SaveDialog.SecondaryButtonText = appsettings.GetResourceString("AskSaveDialog_DontSaveButton/Text");
            }
            return await SaveDialog.ShowAsync();
        }
    }
}
