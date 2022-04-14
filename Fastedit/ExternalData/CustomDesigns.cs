using Fastedit.Core;
using Fastedit.Dialogs;
using Fastedit.Extensions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.ExternalData
{
    public class CustomDesigns
    {
        private readonly AppSettings appsettings = new AppSettings();
        private GridView DesignGridView = null;
        private ContentDialog NameRenameDialog = null;
        private readonly MainPage mainpage = null;
        private readonly muxc.InfoBar infobar = null;
        public StorageFolder DesignsFolder = null;
        public static List<string> DesignItemsForSettings = new List<string>()
        {
            "ThemeIndex","TextSelectionColorIndex","TextColorIndex","TextBackgroundColorIndex",
            "TextColor","TextBackgroundColor","TextSelectionColor","LineNumberBackgroundColorIndex","LineNumberBackgroundColor",
            "LineNumberForegroundColorIndex", "LineNumberForegroundColor", "AppBackgroundColorIndex","AppBackgroundColor",
            "TitleBarBackgroundColor", "TitleBarBackgroundColorIndex",
            "TabColorNotFocusedIndex","TabColorFocusedIndex","TabColorFocused","TabColorNotFocused",
            "DialogBackgroundColor","DialogBackgroundColorIndex", "StatusbarForegroundColorIndex",
            "StatusbarBackgroundColorIndex","StatusbarForegroundColor","StatusbarBackgroundColor", "UseMica","AcrylicEnabled",
            "LineHighlighterBackground", "LineHighlighterForeground", "LineHighlighterForegroundIndex",
            "LineHighlighterBackgroundIndex"
        };

        public CustomDesigns(GridView gridview, MainPage mp, muxc.InfoBar infobar = null)
        {
            SetStorageFolder();
            DesignGridView = gridview;
            mainpage = mp;
            this.infobar = infobar;
        }
        
        private async void SetStorageFolder()
        {
            if (DesignsFolder == null)
                DesignsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.CustomDesigns_FolderName, CreationCollisionOption.OpenIfExists);
        }
        public async void ShowError(string Content, string Title = "", muxc.InfoBarSeverity severity = muxc.InfoBarSeverity.Error)
        {
            if (mainpage == null)
            {
                if(infobar != null)
                {
                    infobar.Message = Content;
                    infobar.Title = Title;
                    infobar.Severity = severity;
                    infobar.IsOpen = true;
                    return;
                }
                await new MessageDialog(Content, Title).ShowAsync();
                return;
            }
            mainpage.ShowInfobar(Content, Title, severity);
        }

        public DesignGridViewItem CreateGridViewItem(StorageFile file)
        {
            var FileContent = File.ReadAllText(file.Path).Split("\n");
            Color BackgroundColor = Extensions.Convert.ToColor(StringBuilder.GetStringFromImportedData(FileContent, "AppBackgroundColor"), DefaultValues.DefaultAppBackgroundColor);
            Color TextColor = Extensions.Convert.ToColor(StringBuilder.GetStringFromImportedData(FileContent, "TextColor"), DefaultValues.DefaultTextColor);
            Color TitlebarColor = Extensions.Convert.ToColor(StringBuilder.GetStringFromImportedData(FileContent, "TitleBarBackgroundColor"), DefaultValues.DefaultTitleBarBackgroundColor);

            return new DesignGridViewItem
            {
                FileName = file.Name,
                Background = new SolidColorBrush(BackgroundColor),
                TextColor = new SolidColorBrush(TextColor),
                TitlebarColor = new SolidColorBrush(TitlebarColor),
                File = file
            };
        }
        public async Task AddAllDesignsToGridView()
        {
            if (DesignsFolder == null)
                DesignsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.CustomDesigns_FolderName, CreationCollisionOption.OpenIfExists);

            int LastSelectedIndex = DesignGridView.SelectedIndex;

            string[] filePaths = Directory.GetFiles(DesignsFolder.Path);
            DesignGridView.Items.Clear();

            for (int i = 0; i < filePaths.Length; i++)
            {
                StorageFile sf = await DesignsFolder.GetFileAsync(Path.GetFileName(filePaths[i]));
                var item = CreateGridViewItem(sf);
                DesignGridView.Items.Add(item);
            }
            DesignGridView.SelectedIndex = LastSelectedIndex;
        }
        public bool DesignIsValid(string content)
        {
            if (content.Length == 0)
                return false;
            string[] contentstr = content.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < contentstr.Length; i++)
            {
                if (contentstr[i].Length == 0 || !contentstr[i].Contains("="))
                {
                    return false;
                }
            }
            return true;
        }
        public string CreateDesignContent()
        {
            string output = "";
            for (int i = 0; i < DesignItemsForSettings.Count; i++)
            {
                string key = DesignItemsForSettings[i];
                output += $"{key}={appsettings.GetSettings(key)}\n";
            }
            return output;
        }
        private void ShowDesignValidInfo(bool IsValid)
        {
            ShowError(IsValid ? "Design changed successfully" : "Design load failed, maybe something is wrong with your file.", "", IsValid ? muxc.InfoBarSeverity.Success : muxc.InfoBarSeverity.Error);
        }
        public async Task<string> ShowRenameDesignUI(string recentname = "")
        {
            var RenameTextBox = new TextBox
            {
                PlaceholderText = appsettings.GetResourceString("RenameThemeDialog_NamePlaceholder/Text"),
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            RenameTextBox.Text = recentname.Replace(DefaultValues.Extension_FasteditDesign, "");
            RenameTextBox.SelectAll();
            RenameTextBox.TextChanged += delegate
            {
                bool res = RenameTextBox.Text.Length > 0 && StringBuilder.IsValidFilename(RenameTextBox.Text);
                RenameTextBox.BorderBrush =
                    res ? DefaultValues.CorrectInput_Color : DefaultValues.WrongInput_Color;
                NameRenameDialog.IsPrimaryButtonEnabled = res;
            };

            NameRenameDialog = new ContentDialog
            {
                Foreground = DefaultValues.ContentDialogForegroundColor(),
                Background = DefaultValues.ContentDialogBackgroundColor(),
                CornerRadius = DefaultValues.DefaultDialogCornerRadius,
                Title = appsettings.GetResourceString("RenameThemeDialog_Title/Text"),
                Content = RenameTextBox,
                PrimaryButtonText = appsettings.GetResourceString("RenameThemeDialog_DoneButton/Text"),
                IsPrimaryButtonEnabled = true,
                CloseButtonText = appsettings.GetResourceString("RenameThemeDialog_CancelButton/Text")
            };
            var dialogres = await NameRenameDialog.ShowAsync();

            if (dialogres == ContentDialogResult.Primary)
            {
                string textboxtext = RenameTextBox.Text;
                if (textboxtext.Length != 0 && StringBuilder.IsValidFilename(textboxtext))
                {
                    if (!await DesignExsists(RenameTextBox.Text + DefaultValues.Extension_FasteditDesign))
                    {
                        RenameTextBox.BorderBrush = DefaultValues.CorrectInput_Color;
                        return Path.GetFileNameWithoutExtension(textboxtext);
                    }
                    else
                    {
                        ShowError("A design with the specified name already exists!", "", muxc.InfoBarSeverity.Warning);
                    }
                }
            }
            return "";
        }

        //loading design
        public async Task<bool> LoadDesignFromFile(StorageFile file)
        {
            if (file != null)
            {
                string designData = await FileIO.ReadTextAsync(file);

                if (DesignIsValid(designData) == true)
                {
                    LoadDesignData(designData.Split("\n"));
                    return true;
                }
            }
            return false;
        }
        public void LoadDesignData(string[] lines)
        {
            for (int i = 0; i < DesignItemsForSettings.Count; i++)
            {
                appsettings.SaveSettings(DesignItemsForSettings[i], StringBuilder.GetStringFromImportedData(lines, DesignItemsForSettings[i]));
            }
        }
        public async Task<bool> DesignExsists(string filename)
        {
            try
            {
                if (filename.Length < 1)
                    return false;
                if (await DesignsFolder.GetFileAsync(filename) != null)
                    return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in CustomDesigns --> DesignExsists:" + "\n" + e.Message);
            }
            return false;
        }

        public async Task<StorageFile> GetFileFromDesignsFolder(string FileName)
        {
            if (FileName.Length == 0)
                return null;
            return await DesignsFolder.GetFileAsync(FileName);
        }
        public static async Task<List<StorageFile>> GetAllInstalledDesigns()
        {
            try
            {
                List<StorageFile> lst = new List<StorageFile>();
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DefaultValues.CustomDesigns_FolderName, CreationCollisionOption.OpenIfExists);
                string[] filePaths = Directory.GetFiles(folder.Path);

                for (int i = 0; i < filePaths.Length; i++)
                {
                    lst.Add(await folder.GetFileAsync(Path.GetFileName(filePaths[i])));
                }
                return lst;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in CustomDesigns --> GetAllInstalledDesigns:" + "\n" + e.Message);
                return null;
            }
        }

        public async Task OnSelectionChange()
        {
            try
            {
                if (appsettings.GetSettingsAsInt("SelectedDesign", 0) != DesignGridView.SelectedIndex && DesignGridView.SelectedIndex > -1)
                {
                    appsettings.SaveSettings("SelectedDesign", DesignGridView.SelectedIndex);

                    if (DesignGridView.SelectedItem is DesignGridViewItem item)
                    {
                        var res = await LoadDesignFromFile(
                            await GetFileFromDesignsFolder(item.FileName));
                        ShowDesignValidInfo(res);
                    }
                    else
                        ShowError("Could not change the design");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error occured while changing the design\n" + ex.Message);
            }
        }

        //design actions:
        public async Task<bool> ImportDesign()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(DefaultValues.Extension_FasteditDesign);

            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

            if (files == null)
                return false;
            bool retval = false;
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file != null)
                {
                    string fileContent = await FileIO.ReadTextAsync(file);

                    bool isvalid = DesignIsValid(fileContent);
                    ShowDesignValidInfo(isvalid);
                    if (isvalid)
                    {
                        await file.CopyAsync(DesignsFolder, file.Name, NameCollisionOption.ReplaceExisting);
                        await AddAllDesignsToGridView();
                        retval = true;
                    }
                    else
                        retval = false;
                }
                else
                    retval = false;
            }
            return retval;
        }
        public async Task<bool> SaveCurrentAsDesign(DesignGridViewItem item, bool UpdateItems = true, string designcontent = "")
        {
            try
            {
                if (item == null)
                    return false;
                StorageFile file = await DesignsFolder.CreateFileAsync(item.FileName, CreationCollisionOption.ReplaceExisting);
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, designcontent == "" ? CreateDesignContent() : designcontent);
                    if(UpdateItems)
                        await AddAllDesignsToGridView();
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowError("Could not save theme\n" + ex.Message);
            }
            return false;
        }
        public async Task CreateNewDesign()
        {
            try
            {
                //just use the renameTheme function to name it
                string designname = await ShowRenameDesignUI();
                if (designname.Length != 0)
                {
                    StorageFile file = await DesignsFolder.CreateFileAsync(designname + DefaultValues.Extension_FasteditDesign, CreationCollisionOption.OpenIfExists);
                    if(file != null)
                    {
                        await FileIO.WriteTextAsync(file, CreateDesignContent());
                        var item = CreateGridViewItem(file);
                        DesignGridView.Items.Add(item);
                        DesignGridView.SelectedItem = item;
                    }
                }

            }
            catch (Exception ex)
            {
                ShowError("Couldn't create a new Design" + ex.Message);
            }
        }
        public async Task ExportDesign(DesignGridViewItem item)
        {
            void ReportError(string content)
            {
                ShowError("Could not export item!\n" + content, "Export error");
            }
            try
            {
                FileSavePicker savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                savePicker.FileTypeChoices.Add("Fasteditdesign", new List<string>() { DefaultValues.Extension_FasteditDesign });
                savePicker.SuggestedFileName = item.FileName;

                StorageFile saveto_file = await savePicker.PickSaveFileAsync();
                if (saveto_file != null)
                {
                    if (item != null)
                    {
                        StorageFile file2 = await DesignsFolder.GetFileAsync(item.FileName);
                        string text = await FileIO.ReadTextAsync(file2);
                        await FileIO.WriteTextAsync(saveto_file, text);
                        ShowError("Successfully exported\n", saveto_file.DisplayName, muxc.InfoBarSeverity.Success);
                    }
                    else
                        ReportError("");
                }
                //Cancel pressed, do nothing here
            }
            catch (Exception ex)
            {
                ReportError(ex.Message);
            }
        }
        public async Task DeleteDesign(DesignGridViewItem item)
        {
            try
            {
                if (item != null)
                {
                    await item.File.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    DesignGridView.Items.Remove(item);
                }
            }
            catch (Exception ex)
            {
                ShowError("Could not delete item!\n" + ex.Message, "Delete error");
            }
        }
        public async Task RenameDesign(DesignGridViewItem item)
        {
            try
            {
                if (item != null)
                {
                    string newname = await ShowRenameDesignUI(item.File.DisplayName);
                    if (newname.Length > 0)
                    {
                        StorageFile file = await DesignsFolder.GetFileAsync(item.FileName);
                        if (file != null)
                        {
                            await file.RenameAsync(newname + DefaultValues.Extension_FasteditDesign, NameCollisionOption.FailIfExists);
                            item.FileName = newname + DefaultValues.Extension_FasteditDesign;
                            await AddAllDesignsToGridView();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
    }
    public class DesignGridViewItem
    {
        private string _FileName = "";
        public string FileName { get => _FileName; set { _FileName = value; DisplayName = value.Replace(DefaultValues.Extension_FasteditDesign, ""); } }
        public StorageFile File { get; set; }
        public SolidColorBrush Background { get; set; }
        public SolidColorBrush TextColor { get; set; }
        public SolidColorBrush TitlebarColor { get; set; }
        public string DisplayName { private set; get; }
    }
}
