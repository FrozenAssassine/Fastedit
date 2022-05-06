using Fastedit.Controls.Textbox;
using Fastedit.Core;
using Fastedit.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Dialogs
{
    public sealed partial class FileInfoDialog : ContentDialog
    {
        ResourceLoader Resloader = ResourceLoader.GetForCurrentView();

        private async Task<string> Createcontent(muxc.TabViewItem Tab)
        {
            if (Tab.Content is TextControlBox textbox)
            {
                if (textbox != null)
                {
                    var Resloader = ResourceLoader.GetForCurrentView();
                    AppSettings appsettings = new AppSettings();
                    StorageFile file = textbox.Storagefile;
                    string tabheader = textbox.Header;
                    string tabtext = textbox.GetText();
                    string FilePath = textbox.FilePath;
                    string FileExtension = Path.GetExtension(textbox.Header);
                    string DataCreatedTime = string.Empty;
                    string NoDataToDisplay = string.Empty;
                    string FileSize = string.Empty;
                    string LastModified = string.Empty;
                    int NbrOfWords = textbox.CountWords(tabtext);
                    FileExtensions fe = new FileExtensions();
                    for (int i = 0; i < fe.FileExtentionList.Count; i++)
                    {
                        if (fe.FileExtentionList[i].Extension.Contains(FileExtension))
                        {
                            FileExtension += " (" + fe.FileExtentionList[i].ExtensionName + ")";
                            break;
                        }
                    }
                    if (file != null)
                    {
                        BasicProperties basicProperties = await file.GetBasicPropertiesAsync();

                        FilePath = textbox.TabSaveMode == TabSaveMode.SaveAsTemp ? NoDataToDisplay : file.Path;
                        DataCreatedTime = file.DateCreated.ToString();
                        LastModified = basicProperties.DateModified.ToString();
                        FileSize = basicProperties.Size.ToString() + "B";
                        if (basicProperties.Size > 1000.0)
                        {
                            FileSize = (basicProperties.Size / 1000.0).ToString() + "KB";
                        }
                        else if (basicProperties.Size == 0)
                        {
                            FileSize = NoDataToDisplay;
                        }
                    }

                    string DialogContent = Resloader.GetString("FileInfo_Filename/Text") + " " + tabheader + "\n" +
                        Resloader.GetString("FileInfo_Filepath/Text") + " " + FilePath + "\n" +
                        Resloader.GetString("FileInfo_Extension/Text") + " " + FileExtension + "\n" +
                        Resloader.GetString("FileInfo_Created/Text") + " " + DataCreatedTime + "\n";
                    if (LastModified != NoDataToDisplay)
                    {
                        DialogContent += Resloader.GetString("FileInfo_LastModified/Text") + " " + DataCreatedTime + "\n";
                    }

                    if (FileSize != NoDataToDisplay)
                    {
                        DialogContent += Resloader.GetString("FileInfo_FileSize/Text") + " " + FileSize + "\n";
                    }

                    DialogContent += Resloader.GetString("FileInfo_WordCount/Text") + " " + NbrOfWords + "\n" +
                        Resloader.GetString("FileInfo_CharacterCount/Text") + " " + tabtext.Length + "\n";

                    DialogContent += Resloader.GetString("FileInfo_Lines/Text") + " " + textbox.GetLinesCount + "\n";

                    return DialogContent;
                }
                return "";
            }
            return "";
        }
        private async void InitDialog(muxc.TabViewItem Tab, TextControlBox textbox)
        {
            this.Background = DefaultValues.ContentDialogBackgroundColor();
            CornerRadius = DefaultValues.DefaultDialogCornerRadius;
            RequestedTheme = DefaultValues.ContentDialogTheme();
            Foreground = DefaultValues.ContentDialogForegroundColor();
            Title = Resloader.GetString("FileInfo_FileInfo/Text") + " " + textbox.Header;
            CloseButtonText = Resloader.GetString("FileInfo_OkButton/Text");
            DataDisplayTextbox.Text = await Createcontent(Tab);
        }
        public FileInfoDialog(muxc.TabViewItem Tab)
        {
            this.InitializeComponent();
            if (Tab.Content is TextControlBox textbox)
            {
                InitDialog(Tab, textbox);
            }
            else
            {
                this.Hide();
            }
        }
    }
}
