using Fastedit.Extensions;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Fastedit.Views.SettingsPage
{
    public class PageSettings
    {
        public static void SelectComboBoxItemByTag(ComboBox combobox, string Tag)
        {
            for (int i = 0; i < combobox.Items.Count; i++)
            {
                if (combobox.Items[i] is ComboBoxItem item)
                {
                    if (item.Tag.Equals(Tag))
                    {
                        combobox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        public static Color SaveCustomAndAccentColorToSettings(string Value, AccentColors accentcolor, Color DefaultColor, ComboBox cb, Controls.ColorChooserButton colorchooserbutton, bool SaveColors = true)
        {
            colorchooserbutton.IsUsedAsDisplay = true;
            AppSettings appsettings = new AppSettings();
            if (cb.SelectedIndex == 0)
            {
                if (SaveColors)
                {
                    appsettings.SaveSettingsAsColor(Value, DefaultColor, accentcolor);
                }
            }
            else
            {
                if (SaveColors)
                {
                    appsettings.SaveSettingsAsColor(Value, colorchooserbutton.Color);
                }

                colorchooserbutton.IsUsedAsDisplay = false;
            }
            colorchooserbutton.Color = appsettings.GetSettingsAsColorWithDefault(Value, DefaultColor);
            return colorchooserbutton.Color;
        }

        /// <summary>
        /// Save color from combobox with values of Custom, Accent, and Transparent color
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="accentcolor"></param>
        /// <param name="DefaultColor"></param>
        /// <param name="cb"></param>
        /// <param name="colorchooserbutton"></param>
        /// <returns></returns>
        public static Color SaveCustomAccentTransparentColorToSettings(string Value, AccentColors accentcolor, Color DefaultColor, ComboBox cb, Controls.ColorChooserButton colorchooserbutton, bool SaveColors = true)
        {
            if (colorchooserbutton != null)
            {
                colorchooserbutton.IsUsedAsDisplay = true;

                AppSettings appsettings = new AppSettings();
                if (cb.SelectedIndex == 0)
                {
                    if (SaveColors)
                    {
                        appsettings.SaveSettingsAsColor(Value, DefaultColor, accentcolor);
                    }
                }
                else if (cb.SelectedIndex == 1)
                {
                    if (SaveColors)
                    {
                        appsettings.SaveSettingsAsColor(Value, Color.FromArgb(0, 255, 255, 255));
                    }
                }
                else
                {
                    if (SaveColors)
                    {
                        appsettings.SaveSettingsAsColor(Value, colorchooserbutton.Color);
                    }

                    colorchooserbutton.IsUsedAsDisplay = false;
                }
                colorchooserbutton.Color = appsettings.GetSettingsAsColorWithDefault(Value, DefaultColor);
                return colorchooserbutton.Color;
            }
            else
            {
                return DefaultColor;
            }
        }
    }
}
