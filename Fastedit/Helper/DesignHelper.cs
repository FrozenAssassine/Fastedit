using Fastedit.Dialogs;
using Fastedit.Models;
using Fastedit.Storage;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Fastedit.Core.Settings;

namespace Fastedit.Helper;

public class DesignHelper
{
    public static FasteditDesign CurrentDesign = null;

    public static FasteditDesign CreateDefaultDesign() 
    {
        return new FasteditDesign
        {
            BackgroundColor = Color.FromArgb(255, 30, 30, 30),
            BackgroundType = BackgroundType.Solid,
            CursorColor = Color.FromArgb(255, 200, 200, 200),
            DialogBackgroundColor = Color.FromArgb(255, 40, 40, 40),
            DialogBackgroundType = ControlBackgroundType.Acrylic,
            DialogTextColor = Color.FromArgb(255, 220, 220, 220),
            LineHighlighterBackground = Color.FromArgb(100, 50, 50, 50),
            LineNumberBackground = Color.FromArgb(255, 35, 35, 35),
            LineNumberColor = Color.FromArgb(255, 150, 150, 150),
            SearchHighlightColor = Color.FromArgb(100, 255, 200, 0),
            SelectedTabPageHeaderBackground = Color.FromArgb(255, 45, 45, 45),
            SelectedTabPageHeaderTextColor = Color.FromArgb(255, 220, 220, 220),
            SelectionColor = Color.FromArgb(100, 100, 100, 100),
            StatusbarBackgroundColor = Color.FromArgb(255, 30, 30, 30),
            StatusbarBackgroundType = ControlBackgroundType.Acrylic,
            StatusbarTextColor = Color.FromArgb(255, 200, 200, 200),
            TextBoxBackground = Color.FromArgb(255, 35, 35, 35),
            TextboxBackgroundType = ControlBackgroundType.Acrylic,
            TextColor = Color.FromArgb(255, 230, 230, 230),
            Theme = ElementTheme.Dark,
            UnselectedTabPageHeaderBackground = Color.FromArgb(255, 25, 25, 25),
            UnSelectedTabPageHeaderTextColor = Color.FromArgb(255, 160, 160, 160),
        };
    }

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
            CurrentDesign = CreateDefaultDesign();
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

    public static string DuplicateDesign(string designName)
    {
        if (string.IsNullOrEmpty(designName))
            return null;

        var sourcePath = Path.Join(DefaultValues.DesignPath, designName);

        var path = Path.GetDirectoryName(sourcePath);
        var duplicateFileName = SaveFileHelper.GenerateUniqueNameFromPath(Path.Join(path, designName));

        File.Copy(sourcePath, Path.Join(path, duplicateFileName));
        return duplicateFileName;
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
        var design = CreateDefaultDesign();
        if(SaveDesign(design, path))
            return path;
        return null;
    }
    
    public static bool SaveDesign(FasteditDesign design, string path)
    {
        try
        {
            string data = JsonConvert.SerializeObject(design, Formatting.Indented);
            File.WriteAllText(path, data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Brush CreateBackgroundBrush(Color color, ControlBackgroundType type)
    {
        double transparency = color.A / 255.0;
        if (type == ControlBackgroundType.Acrylic)
        {
            color.A = 255;
            return new AcrylicBrush
            {
                TintColor = color,
                TintOpacity = transparency,
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
        var newFile = await SaveFileHelper.PickFile(".json", "Json", designName);
        if (newFile.Length == 0)
            return true;

        try
        {
            var currentFileContent = File.ReadAllText(Path.Combine(DefaultValues.DesignPath, designName));
            File.WriteAllText(newFile, currentFileContent);
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
    public static bool DeleteDesign(string designName, GridView designGridView)
    {
        if (designName.Equals(AppSettings.CurrentDesign))
        {
            var files = Directory.GetFiles(DefaultValues.DesignPath);
            if (files.Length <= 1)
            {
                InfoMessages.OneDesignMustRemain();
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
            File.Delete(Path.Combine(DefaultValues.DesignPath, designName));
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
    Acrylic, Solid, Mica
}
public enum ControlBackgroundType
{
    Acrylic, Solid, Null
}
