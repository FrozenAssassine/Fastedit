using Fastedit.Storage;
using Fastedit.Tab;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fastedit.Settings
{
    internal class SettingsImportExport
    {
        public static async Task<bool> Export()
        {
            var file = await SaveFileHelper.PickFile("settings.fasteditsettings", ".fasteditsettings", "Fastedit settings");
            if (file == null)
                return false;

            FieldInfo[] fieldInfos = typeof(AppSettingsValues).GetFields(BindingFlags.Public |
                     BindingFlags.Static | BindingFlags.FlattenHierarchy);

            StringBuilder data = new StringBuilder();
            foreach (var item in fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly))
            {
                data.AppendLine(item.GetValue(null) + "=" + AppSettings.GetSettings(item.GetValue(null).ToString()));
            }

            await FileIO.WriteTextAsync(file, data.ToString());
            return true;
        }

        public static async Task<bool> Import()
        {
            var file = await OpenFileHelper.PickFile(".fasteditsettings");
            if (file == null)
                return false;

            foreach (var line in await FileIO.ReadLinesAsync(file))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length > 0)
                {
                    var splitted = trimmedLine.Split("=", StringSplitOptions.RemoveEmptyEntries);
                    if (splitted.Length > 1)
                    {
                        AppSettings.SaveSettings(splitted[0], splitted[1]);
                    }
                }
            }

            //Apply the imported settings
            TabPageHelper.mainPage.ApplySettings();
            return true;
        }
    }
}
