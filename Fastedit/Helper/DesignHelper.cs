using Fastedit.Dialogs;
using Fastedit.Models;
using Fastedit.Storage;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinUIEx;
using System.Diagnostics;
using Fastedit.Core.Settings;

namespace Fastedit.Helper
{
    public class DesignHelper
    {
        public static FasteditDesign CurrentDesign = null;

        public static void LoadDesign()
        {
            var designName = AppSettings.CurrentDesign;
            string path = Path.Combine(DefaultValues.DesignPath, designName);

            if (File.Exists(path))
            {
                CurrentDesign = GetDesignFromFile(path);
            }
            //if the design could not get loaded, load alternative design:
            if (CurrentDesign == null)
                CurrentDesign = LoadDefaultDesign();

            //if the design still could not get loaded, load some data
            if (CurrentDesign == null)
            {
                CurrentDesign = new FasteditDesign
                {
                    BackgroundColor = Color.FromArgb(100, 0, 0, 0),
                    BackgroundType = BackgroundType.Solid,
                    CursorColor = Color.FromArgb(255, 255, 255, 255),
                    DialogBackgroundColor = Color.FromArgb(30, 20, 20, 20),
                    DialogBackgroundType = ControlBackgroundType.Acrylic,
                    DialogTextColor = Color.FromArgb(255, 255, 255, 255),
                    LineHighlighterBackground = Color.FromArgb(50, 0, 0, 0),
                    LineNumberBackground = Color.FromArgb(0, 0, 0, 0),
                    LineNumberColor = Color.FromArgb(0, 0, 0, 0),
                    SearchHighlightColor = Color.FromArgb(0, 0, 0, 0),
                    SelectedTabPageHeaderBackground = Color.FromArgb(0, 0, 0, 0),
                    SelectedTabPageHeaderTextColor = Color.FromArgb(0, 0, 0, 0),
                    SelectionColor = Color.FromArgb(0, 0, 0, 0),
                    StatusbarBackgroundColor = Color.FromArgb(0, 0, 0, 0),
                    StatusbarBackgroundType = ControlBackgroundType.Acrylic,
                    StatusbarTextColor = Color.FromArgb(0, 0, 0, 0),
                    TextBoxBackground = Color.FromArgb(0, 0, 0, 0),
                    TextboxBackgroundType = ControlBackgroundType.Acrylic,
                    TextColor = Color.FromArgb(0, 0, 0, 0),
                    Theme = ElementTheme.Dark,
                    UnselectedTabPageHeaderBackground = Color.FromArgb(0, 0, 0, 0),
                    UnSelectedTabPageHeaderTextColor = Color.FromArgb(0, 0, 0, 0),
                };
            }
        }
        public static void CopyDefaultDesigns(bool force = false)
        {
            if (!force && AppSettings.DesignLoaded)
                return;

            AppSettings.DesignLoaded = true;

            try
            {
                if(!force && Directory.Exists(DefaultValues.DesignPath) && Directory.GetFiles(DefaultValues.DesignPath).Length > 0)
                {
                    //the directory already contain designs
                    return;
                }

                string installedLocation = $"{Windows.ApplicationModel.Package.Current.InstalledLocation.Path}\\Assets\\Designs";
                foreach (var file in Directory.GetFiles(installedLocation))
                {
                    if (string.IsNullOrEmpty(file))
                        continue;

                    File.Copy(file, Path.Combine(DefaultValues.DesignPath, Path.GetFileName(file)), true);
                }
            }
            catch
            {
                AppSettings.DesignLoaded = false;
            }
        }

        public static string[] GetDesignsFilesFromFolder()
        {
            if (!Directory.Exists(DefaultValues.DesignPath))
            {
                AppSettings.DesignLoaded = false;
                CopyDefaultDesigns();
                return null;
            }

            return Directory.GetFiles(DefaultValues.DesignPath);
        }

        public static string GetFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }
        public static string GetDesignNameFromPath(string path)
        {
            return Path.GetFileNameWithoutExtension(path).Replace("_", " ");
        }

        private static bool IsValidDesign(string file)
        {
            if (file == null)
                return false;

            try
            {
                return JsonConvert.DeserializeObject<FasteditDesign>(File.ReadAllText(file)) != null;
            }
            catch (Exception ex)
            {
                InfoMessages.DesignLoadError(Path.GetFileName(file), ex);
            }

            return false;
        }
        public static FasteditDesign GetDesignFromFile(string path)
        {
            if (!Path.GetExtension(path).Equals(".json") || !File.Exists(path))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<FasteditDesign>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                InfoMessages.DesignLoadError(GetFileNameFromPath(path), ex);
                return null;
            }
        }

        public static FasteditDesign LoadDefaultDesign()
        {
            string installedLocation = $"{Windows.ApplicationModel.Package.Current.InstalledLocation.Path}\\Assets\\Designs" + DefaultValues.DefaultDesignName;
            try
            {
                return JsonConvert.DeserializeObject<FasteditDesign>(File.ReadAllText(installedLocation));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static bool IsValidDesignName(string name)
        {
            var invalid = Path.GetInvalidPathChars();
            for (int i = 0; i < name.Length; i++)
            {
                if (invalid.Contains(name[i]))
                    return false;
            }
            return true;
        }

        public static string CreateDesign(string name)
        {
            if (name == null)
                return null;

            var path = Path.Combine(DefaultValues.DesignPath, name);
            var design = new FasteditDesign();
            if(SaveDesign(design, path))
                return path;
            return null;
        }
        
        public static bool SaveDesign(FasteditDesign design, string path)
        {
            try
            {
                string data = JsonConvert.SerializeObject(design);
                File.WriteAllText(path, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetBackground(Window window, Color color, BackgroundType type)
        {
            if (window == null)
                return;


            if (type == BackgroundType.Acrylic)
            {
                Debug.WriteLine("Try acrylic");
                 BackdropHelper.TrySetAcrylicBackdrop(window);
            }
            else if (type == BackgroundType.Solid)
            {
                Debug.WriteLine("Try solid");

                //TODO!
                //= new SolidColorBrush { Color = color };
            }
            else if (type == BackgroundType.Mica)
            {
                Debug.WriteLine("Try acrylic");

                BackdropHelper.TrySetMicaBackdrop(window);
            }
            else if(type == BackgroundType.Transparent)
            {
                Debug.WriteLine("Try Transparent");

                window.SystemBackdrop = new TransparentTintBackdrop { TintColor = color };
            }
        }

        public static void SetBackground(Control element, Color color, BackgroundType type)
        {
            if (element == null)
                return;

            //remove mica
            int transparency = color.A;
            color.A = 255;
            if (type == BackgroundType.Transparent)
            {
                element.Background = null;
            }
            else if (type == BackgroundType.Acrylic)
            {
                element.Background = new AcrylicBrush
                {
                    TintColor = color,
                    TintOpacity = transparency / 255.0,
                    FallbackColor = color,
                };
            }
            else if (type == BackgroundType.Solid)
            {
                element.Background = new SolidColorBrush { Color = color };
            }
        }
        public static Brush CreateBackgroundBrush(Color color, ControlBackgroundType type)
        {
            int transparency = color.A;
            color.A = 255;

            if (type == ControlBackgroundType.Acrylic)
            {
                return new AcrylicBrush
                {
                    TintColor = color,
                    TintOpacity = transparency / 255.0,
                    FallbackColor = color,
                };
            }
            else if (type == ControlBackgroundType.Solid)
            {
                return new SolidColorBrush { Color = color };
            }
            return null;
        }

        public static async Task<bool> ExportDesign(string designName)
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(DefaultValues.DesignPath, designName));
            if (file == null)
                return false; 
            
            var newFile = await SaveFileHelper.PickFile(".json", "Json");
            if (newFile.Length == 0)
                return true; //no file was picked

            try
            {
                File.WriteAllText(newFile, await FileIO.ReadTextAsync(file));
                return true;
            }
            catch { }
            return false;
        }
        public static async Task<bool> ImportDesign()
        {
            var file = await OpenFileHelper.PickFile(".json");
            if (file.Length == 0)
                return true; //no file was picked

            //validate the design
            if (!IsValidDesign(file))
                return false;
            try
            {
                string fileName = SaveFileHelper.GenerateUniqueNameFromPath(Path.Join(DefaultValues.DesignPath, Path.GetFileName(file)));
                File.Copy(file, Path.Join(DefaultValues.DesignPath, fileName));
                return true;
            }
            catch { }
            return false;
        }
        public static async Task<bool> DeleteDesign(string designName, GridView designGridView)
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(DefaultValues.DesignPath, designName));
            if (file == null)
                return false;

            //when the active design was deleted
            if (designName.Equals(AppSettings.CurrentDesign))
            {
                var files = Directory.GetFiles(DefaultValues.DesignPath);
                if (files.Length <= 1)
                {
                    InfoMessages.OneDesignNeedsToBeLeft();
                    return true;
                }

                //select the item
                designGridView.SelectedItem = designGridView.Items.FirstOrDefault(x => (x as DesignGridViewItem).FileName.Equals(designName));

                AppSettings.CurrentDesign = Path.GetFileName(files[0]);
            }
            if(designGridView.Items.Count  == 1)
                designGridView.SelectedIndex = 0;
            try
            {
                await file.DeleteAsync();
                return true;
            }
            catch { }
            return false;
        }
    }
    public class FasteditDesign 
    {
        public ElementTheme Theme { get; set; }
        public Color? BackgroundColor { get; set; }
        public Color? TextColor { get; set; }
        public Color? SelectionColor { get; set; }
        public BackgroundType BackgroundType { get; set; }
        public Color? LineNumberColor { get; set; }
        public Color? LineNumberBackground { get; set; }
        public Color? LineHighlighterBackground { get; set; }
        public Color? TextBoxBackground { get; set; }
        public Color? CursorColor { get; set; }
        public Color? SearchHighlightColor { get; set; }
        public ControlBackgroundType TextboxBackgroundType { get; set; }
        public Color? SelectedTabPageHeaderTextColor { get; set; }
        public Color? UnSelectedTabPageHeaderTextColor { get; set; }
        public Color? UnselectedTabPageHeaderBackground { get; set; }
        public Color? SelectedTabPageHeaderBackground { get; set; }
        public Color? StatusbarTextColor { get; set; }
        public ControlBackgroundType StatusbarBackgroundType { get; set; }
        public Color? StatusbarBackgroundColor { get; set; }
        public Color? DialogBackgroundColor { get; set; }
        public Color? DialogTextColor { get; set; }
        public ControlBackgroundType DialogBackgroundType { get; set; }
    }
    public enum BackgroundType
    {
        Acrylic, Solid, Mica, Transparent
    }
    public enum ControlBackgroundType
    {
        Acrylic, Solid, Null
    }
}
