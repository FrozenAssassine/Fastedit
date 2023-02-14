using Fastedit.Dialogs;
using Fastedit.Helper;
using Fastedit.Settings;
using Fastedit.Tab;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Views
{
    public sealed partial class DesignEditor : Page
    {
        FasteditDesign currentDesign = null;
        AppWindow window = null;

        public DesignEditor(AppWindow window, FasteditDesign design, string designName)
        {
            this.InitializeComponent();
            this.currentDesign = design;
            this.CurrentDesignName = designName;
            this.window = window;

            window.Title = "Edit design " + designName;
        }
        public string CurrentDesignName { get; private set; }
        public bool NeedSave { get; private set; } = false;
        public async Task<bool> SaveDesign()
        {
            StorageFile designFile = await StorageFile.GetFileFromPathAsync(Path.Combine(DefaultValues.DesignPath, CurrentDesignName));
            if (designFile == null)
                return false;

            return await DesignHelper.SaveDesign(currentDesign, designFile);
        }
        //Change events:
        private void Theme_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                currentDesign.Theme = (ElementTheme)Enum.Parse(typeof(ElementTheme), cb.SelectedIndex.ToString());
                NeedSave = true;
            }
        }
        private void BackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.BackgroundColor = args.Color;
            NeedSave = true;
        }
        private void TextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.TextColor = args.Color;
            NeedSave = true;
        }
        private void SelectionColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.SelectionColor = args.Color;
            NeedSave = true;
        }
        private void LinenumberColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.LineNumberColor = args.Color;
            NeedSave = true;
        }
        private void LinenumberBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.LineNumberBackground = args.Color;
            NeedSave = true;
        }
        private void LinehighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.LineHighlighterBackground = args.Color;
            NeedSave = true;
        }
        private void TextboxBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.TextBoxBackground = args.Color;
            NeedSave = true;
        }
        private void TextboxBackgroundtype_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                currentDesign.TextboxBackgroundType = (ControlBackgroundType)Enum.Parse(typeof(ControlBackgroundType), cb.SelectedIndex.ToString());
                NeedSave = true;
            }
        }
        private void SearchHighlightColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.SearchHighlightColor = args.Color;
            NeedSave = true;
        }
        private void CursorColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.CursorColor = args.Color;
            NeedSave = true;
        }
        private void SelectedTabTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.SelectedTabPageHeaderTextColor = args.Color;
            NeedSave = true;
        }
        private void UnselectedTabTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.UnSelectedTabPageHeaderTextColor = args.Color;
            NeedSave = true;
        }
        private void SelectedTabBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.SelectedTabPageHeaderBackground = args.Color;
            NeedSave = true;
        }
        private void UnselectedTabBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.UnselectedTabPageHeaderBackground = args.Color;
            NeedSave = true;
        }
        private void StatusbarTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.StatusbarTextColor = args.Color;
            NeedSave = true;
        }
        private void StatusbarBackgroundType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                currentDesign.StatusbarBackgroundType = (ControlBackgroundType)Enum.Parse(typeof(ControlBackgroundType), cb.SelectedIndex.ToString());
                NeedSave = true;
            }
        }
        private void StatusbarBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.StatusbarBackgroundColor = args.Color;
            NeedSave = true;
        }
        private void DialogBackgroundType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                currentDesign.DialogBackgroundType = (ControlBackgroundType)Enum.Parse(typeof(ControlBackgroundType), cb.SelectedIndex.ToString());
                NeedSave = true;
            }
        }
        private void DialogBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.DialogBackgroundColor = args.Color;
            NeedSave = true;
        }
        private void DialogTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
        {
            currentDesign.DialogTextColor = args.Color;
            NeedSave = true;
        }

        private void BackgroundType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                currentDesign.BackgroundType = (BackgroundType)Enum.Parse(typeof(BackgroundType), cb.SelectedIndex.ToString());
                NeedSave = true;
            }
        }

        private void ApplyDesign_Click(object sender, RoutedEventArgs e)
        {
            //The design is already selected
            if (!AppSettings.GetSettings(AppSettingsValues.Settings_DesignName).Equals(CurrentDesignName))
            {
                AppSettings.SaveSettings(AppSettingsValues.Settings_DesignName, CurrentDesignName);
            }

            DesignHelper.CurrentDesign = currentDesign;
            TabPageHelper.mainPage.ApplySettings(currentDesign);
        }

        private async void SaveDesign_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDesign())
                InfoMessages.SaveDesignSucceed();
            else
                InfoMessages.SaveDesignError();
        }
    }
}
