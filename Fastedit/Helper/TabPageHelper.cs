using Fastedit.Controls.Textbox;
using Fastedit.Core;
using Fastedit.Extensions;
using System.IO;
using System.Text;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Helper
{
    public class TabPageHelper
    {
        private AppSettings appsettings = new AppSettings();

        public TextControlBox GetTextBoxFromTabPage(muxc.TabViewItem TabPage)
        {
            if (TabPage != null)
            {
                if (TabPage.Content is TextControlBox tb)
                    return tb;
            }
            return null;
        }

        //SetTab Properties
        public void SetTabPath(muxc.TabViewItem TabPage, string FilePath)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).FilePath = FilePath;
            }
        }
        public void SetTabDataBaseName(muxc.TabViewItem TabPage, string DataBaseName)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).DataBaseName = DataBaseName;
            }
        }
        public void SetTabHeader(muxc.TabViewItem TabPage, string newTitle)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                if (TabPage.Header.ToString().Contains("*"))
                {
                    TabPage.Header = newTitle + "*";
                }
                else
                {
                    TabPage.Header = newTitle;
                }
                GetTextBoxFromTabPage(TabPage).Header = newTitle;
            }
        }
        public void SetTabModified(muxc.TabViewItem TabPage, bool isModified, bool SetModified = true)
        {
            if (GetTextBoxFromTabPage(TabPage) is TextControlBox textbox)
            {
                if (SetModified)
                {
                    textbox.IsModified = isModified;
                }

                var TabHeader = TabPage.Header.ToString();

                if (isModified)
                {
                    if (!TabHeader.Contains("*"))
                    {
                        TabHeader += "*";
                    }
                }
                else
                {
                    TabHeader = TabHeader.Replace("*", "");
                }

                TabPage.Header = TabHeader;
            }
        }
        public void SetTabStorageFile(muxc.TabViewItem TabPage, StorageFile file)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).Storagefile = file;
            }
        }
        public void SetTabToken(muxc.TabViewItem TabPage, string Token)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).FileToken = Token;
            }
        }
        public void SetTabTemp(muxc.TabViewItem TabPage, string TempFile)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).TempFile = TempFile;
            }
        }
        public void SetTabReadOnly(muxc.TabViewItem TabPage, bool Readonly)
        {
            var tb = GetTextBoxFromTabPage(TabPage);
            if (tb != null)
            {
                tb.IsReadOnly = Readonly;

                if (tb.IsReadOnly)
                    TabPage.IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.ProtectedDocument };
                else
                {
                    TabPage.IconSource = new muxc.FontIconSource
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = appsettings.GetSettingsAsString("TabIconId", DefaultValues.DefaultTabIconId)
                    };
                }
            }
        }
        public void SetTabSaveMode(muxc.TabViewItem TabPage, TabSaveMode TabSaveMode)
        {
            var tb = GetTextBoxFromTabPage(TabPage);
            if (tb != null)
            {
                tb.TabSaveMode = TabSaveMode;
            }
        }
        public void SetTabEncoding(muxc.TabViewItem TabPage, Encoding encoding)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).Encoding = encoding;
            }
        }
        public async void SetTabText(muxc.TabViewItem TabPage, string text)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                await GetTextBoxFromTabPage(TabPage).SetText(text);
            }
        }
        public void SetTextBoxTextBevoreLastSaved(muxc.TabViewItem TabPage, string text)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).TextBeforeLastSaved = text;
            }
        }

        //GetTab Properties
        public string GetTabName(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).Name;
            }

            return string.Empty;
        }
        public bool GetIsModified(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).IsModified;
            }
            return false;
        }
        public string GetDataBaseName(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).DataBaseName;
            }

            return string.Empty;
        }
        public string GetTabFilepath(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).FilePath;
            }
            return string.Empty;
        }
        public string GetTabTempfile(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).TempFile;
            }

            return string.Empty;
        }
        public bool GetTabReadOnly(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).IsReadOnly;
            }
            return false;
        }
        public string GetTabToken(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).FileToken;
            }
            return string.Empty;
        }
        public TabSaveMode GetTabSaveMode(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).TabSaveMode;
            }
            return TabSaveMode.SaveAsTemp;
        }
        public string GetFileExtension(muxc.TabViewItem TabPage)
        {
            TextControlBox tb = GetTextBoxFromTabPage(TabPage);
            if (tb != null)
            {
                if (tb.Storagefile != null)
                {
                    if (tb.Storagefile.FileType.Length != 0)
                        return tb.Storagefile.FileType;
                }
                else if (tb.TempFile.Length > 0)
                {
                    return Path.GetExtension(tb.TempFile);
                }
                return Path.GetExtension(tb.Header);
            }
            return string.Empty;
        }
        public string GetTabHeader(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).Header;
            }

            return string.Empty;
        }
        public Encoding GetTabEncoding(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).Encoding;
            }
            return Encoding.Default;
        }
        public string GetTabText(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).GetText();
            }
            return string.Empty;
        }
        public StorageFile GetTabStorageFile(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                return GetTextBoxFromTabPage(TabPage).Storagefile;
            }
            return null;
        }
        public string GetPathFromStorageFile(StorageFile file)
        {
            if (file != null)
            {
                return file.Path;
            }

            return string.Empty;
        }
        public string GetTabFileName(muxc.TabViewItem TabPage)
        {
            if (TabPage.Content is TextControlBox textbox)
            {
                return textbox.Header;
            }

            return string.Empty;
        }
        public void SetUnsetLockFile(muxc.TabViewItem Tab)
        {
            if (Tab != null)
            {
                SetTabReadOnly(Tab, !GetTabReadOnly(Tab));
            }
        }
        public void ZoomIn(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).ZoomIn(DefaultValues.DefaultZoomFactor);
            }
        }
        public void ZoomOut(muxc.TabViewItem TabPage)
        {
            if (GetTextBoxFromTabPage(TabPage) != null)
            {
                GetTextBoxFromTabPage(TabPage).ZoomOut(DefaultValues.DefaultZoomFactor);
            }
        }


    }
}
