using Fastedit.Models;
using Fastedit.Settings;
using Fastedit.Tab;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fastedit.Helper
{
    public class DesignGridViewHelper
    {
        //Manage the GridView with design items
        public static DesignGridViewItem CreateItem(FasteditDesign design, string designName)
        {
            return new DesignGridViewItem
            {
                TextColor = new SolidColorBrush(ConvertHelper.ToColor(design.TextColor)),
                AppBackground = design.BackgroundType == BackgroundType.Null ? null : new SolidColorBrush(design.BackgroundType == BackgroundType.Mica ? ConvertHelper.GetColorFromTheme(design.Theme) : ConvertHelper.ToColor(design.BackgroundColor)),
                TabPageBackground = new SolidColorBrush(ConvertHelper.ToColor(design.SelectedTabPageHeaderBackground)),
                DesignName = designName,
                LineNumberBackground = new SolidColorBrush(ConvertHelper.ToColor(design.LineNumberBackground)),
                LineNumberColor = new SolidColorBrush(ConvertHelper.ToColor(design.LineNumberColor)),
                TextBoxBackground = new SolidColorBrush(ConvertHelper.ToColor(design.TextBoxBackground)),
                Width = 200,
                Height = 130
            };
        }
        public static async Task LoadItems(GridView designGridView)
        {
            if (!Directory.Exists(DefaultValues.DesignPath))
            {
                AppSettings.SaveSettings(AppSettingsValues.DesignLoaded, "0");
                await DesignHelper.CopyDefaultDesigns();
                return;
            }

            var files = Directory.GetFiles(DefaultValues.DesignPath);

            for (int i = 0; i < files.Length; i++)
            {
                var design = DesignHelper.GetDesignFromFile(files[i]);
                if (design != null)
                {
                    string name = DesignHelper.GetDesingNameFromPath(files[i]);
                    designGridView.Items.Add(CreateItem(design, name));

                    if (AppSettings.GetSettings(AppSettingsValues.Settings_DesignName) == name)
                    {
                        designGridView.SelectedIndex = i >= designGridView.Items.Count ? 0 : i;
                    }
                }
            }
        }
        public static void GridViewClick(ItemClickEventArgs e)
        {
            if (e.ClickedItem is DesignGridViewItem item && item != null)
            {
                AppSettings.SaveSettings(AppSettingsValues.Settings_DesignName, item.DesignName);
                TabPageHelper.mainPage.ApplySettings();

                CursorHelper.SetArrow();
            }
        }
        public static async void UpdateItems(GridView designGridView)
        {
            designGridView.Items.Clear();
            await LoadItems(designGridView);
        }
    }
}
