using Fastedit.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Fastedit.ExternalData
{
    public class ExportImportSettings
    {
        private AppSettings appsettings = new AppSettings();

        List<string> SettingsItems = new List<string>()
        {
            "FontSizeIndex",
            "ShowLineNumbers",
            "ShowDropdown",
            "AppLanguage",
            "TabSizeModeIndex",
            "NewTabTitleName",
            "LoadRecentTabs",
            "HideGoToLineDialogAfterEntering",
            "SelectLineAfterGoingToIt",
            "SelectedDesign",
            "ShowStatusbar",
            "StatusbarInBoldFont",
            "ShowLinenumberButtonOn_SBar",
            "ShowRenameButtonOn_SBar",
            "ShowZoomButtonOn_SBar",
            "ShowEncodingButtonOn_SBar", 
            "ShowSaveStatusButtonOn_SBar",
            "ShowWordCountButtonOn_SBar",
            "ShowNavigateToNextTab",
            "ShowNavigateToPreviousTab",
            "ThemeForLightMode",
            "ThemeForDarkMode",
            "AutomaticThemeChange",
            "TabIconId", 
            "TextboxShowSelectionFlyout",
            "HandwritingEnabled",
            "SearchPanelCenterAlign",
            "LineHighlighter", 
            "MenuBarAlignment",
            "ShowMenubar"
        };
        List<string> AllItems = new List<string>();

        public ExportImportSettings()
        {
            if(AllItems.Count == 0)
            {
                AllItems.AddRange(SettingsItems);
                AllItems.AddRange(CustomDesigns.DesignItemsForSettings);
            }
        }

        public string SetExportData()
        {
            string output = "";
            for (int i = 0; i < AllItems.Count; i++)
            {
                output += $"{AllItems[i]}={appsettings.GetSettings(AllItems[i])}\n";
            }

            return output;
        }
        private void LoadImportedData(string[] lines)
        {
            for (int i = 0; i < AllItems.Count; i++)
            {
                string key = AllItems[i];
                appsettings.SaveSettings(key, StringBuilder.GetStringFromImportedData(lines, key));
            }
        }

        public async Task<bool> ImportSettings()
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                picker.FileTypeFilter.Add(".fasteditsettings");

                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    string text = await FileIO.ReadTextAsync(file);
                    if (text.Length != 0)
                    {
                        string[] lines = text.Split("\n");
                        LoadImportedData(lines);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ExportImportSettings -> ImportSettings\n" + ex.Message);
            }
            return false;
        }
        public async Task<bool> ExportSettings()
        {
            try
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("FasteditSettings", new List<string>() { ".fasteditsettings" });
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, SetExportData());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ExportImportSettings --> ExportSettings:" + "\n" + ex.Message);
            }
            return false;
        }
    }
}