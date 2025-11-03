using Fastedit.Core.Storage;
using Fastedit.Core.Tab;
using Fastedit.Models;
using Fastedit.Storage;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Core.Settings;

internal class SettingsImportExport
{
    public static async Task<SettingsImportExportResult> Export()
    {
        var file = await SaveFileHelper.PickFile(
            ".fasteditsettings", 
            "Fastedit settings", 
            "Settings_" + DateTime.Now.ToString("dd.MM.yyyy")
            );

        if (file.Length == 0)
            return SettingsImportExportResult.Cancelled;

        FieldInfo[] fieldInfos = typeof(AppSettingsValues).GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

        StringBuilder data = new StringBuilder();
        foreach (var item in fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly))
        {
            data.AppendLine(item.GetValue(null) + "=" + SettingsManager.GetSettings(item.GetValue(null).ToString()));
        }

        return await SaveFileHelper.WriteTextToFileAsync(file, data.ToString(), Encoding.UTF8) ? SettingsImportExportResult.Success : SettingsImportExportResult.Failed;
    }

    public static async Task<SettingsImportExportResult> Import()
    {
        var file = await OpenFileHelper.PickFile(".fasteditsettings");
        if (file.Length == 0)
            return SettingsImportExportResult.Cancelled;

        var result = OpenFileHelper.ReadLinesFromFile(file);
        if (!result.succeeded)
            return SettingsImportExportResult.Failed;

        foreach (var line in result.lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.Length == 0)
                continue;

            var splitted = trimmedLine.Split("=", StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                SettingsManager.SaveSettings(splitted[0], splitted[1]);
            }
        }

        //Apply the imported settings
        TabPageHelper.mainPage.ApplySettings();
        return SettingsImportExportResult.Success;
    }
}
