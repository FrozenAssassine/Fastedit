using Fastedit.Core;
using Fastedit.Extensions;
using Fastedit.Views.SettingsPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fastedit.Controls
{
    public sealed partial class CustomizationControl : UserControl
    {
        private AppSettings appsettings = new AppSettings();

        public bool IsAlphaEnabled { set => ColorPickerButton.IsAlphaEnabled = value; }
        public string Header { get; set; } = "";
        public Visibility TransparentComboboxItem
        { 
            get => Cb_Item_Transparent.Visibility;
            set => Cb_Item_Transparent.Visibility = value;
        }
        public string ResourceKey { get; set; } = "";
        public AccentColors Accentcolor { get; set; } = AccentColors.Default;
        public bool SaveChangesToSettings { get; set; } = true;

        public event ColorChanged ColorChangedEvent;
        public delegate void ColorChanged();

        public CustomizationControl()
        {
            this.InitializeComponent();
        }

        public void RetrieveFromSettings()
        {
            ColorPickerButton.Color = appsettings.GetSettingsAsColorWithDefault(ResourceKey, DefaultValues.SystemAccentColor);
            combobox.SelectedIndex = appsettings.GetSettingsAsInt(ResourceKey + "Index", 0);
        }

        private void ColorPickerButton_ColorChangedEvent(ColorPicker sender)
        {
            appsettings.SaveSettings(ResourceKey, sender.Color);
            ColorChangedEvent?.Invoke();
        }

        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorPickerButton.IsUsedAsDisplay = true;
            if (TransparentComboboxItem == Visibility.Visible)
            {
                PageSettings.SaveCustomAccentTransparentColorToSettings(
                   ResourceKey, Accentcolor, Colors.Transparent,
                   combobox, ColorPickerButton, SaveChangesToSettings
                );
            }
            else
            {
                PageSettings.SaveCustomAndAccentColorToSettings(
                   ResourceKey, Accentcolor, Colors.Transparent,
                   combobox, ColorPickerButton, SaveChangesToSettings
               );
            }
            ColorChangedEvent?.Invoke();
            appsettings.SaveSettings(ResourceKey + "Index", combobox.SelectedIndex);
        }
    }
}
