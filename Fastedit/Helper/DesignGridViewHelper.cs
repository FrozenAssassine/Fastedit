using Fastedit.Settings;
using Fastedit.Tab;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fastedit.Helper
{
    public class DesignGridViewHelper
    {
        //Manage the GridView with design items
        public static DesignGridViewItem CreateItem(FasteditDesign design, string designName, int width = 200, int height = 130)
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
                Width = width,
                Height = height
            };
        }
        public static async Task LoadItems(GridView designGridView, int width = 200, int height = 130)
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
                    designGridView.Items.Add(CreateItem(design, name, width, height));

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
            }
        }
        public static async void UpdateItems(GridView designGridView)
        {
            designGridView.Items.Clear();
            await LoadItems(designGridView);
        }
    }

    public class DesignGridViewItem
    {
        public Brush AppBackground { get; set; }
        public Brush TextColor { get; set; }
        public Brush LineNumberColor { get; set; }
        public Brush TextBoxBackground { get; set; }
        public Brush LineNumberBackground { get; set; }
        public Brush TabPageBackground { get; set; }
        public string DesignName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
