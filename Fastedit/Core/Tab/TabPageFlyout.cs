using Fastedit.Controls.Textbox;
using Fastedit.Dialogs;
using Fastedit.Helper;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Core.Tab
{
    public class TabPageFlyout
    {
        public static MenuFlyout CreateFlyoutForTab(muxc.TabView tabview, MainPage mainpage, object sender, bool ForLockedTab)
        {
            if (sender is muxc.TabViewItem TabPage)
            {
                if (TabPage.Content is TextControlBox tb)
                {
                    if (tb == null) return null;

                    var tabactions = new TabActions(tabview);
                    var tabpagehelper = new TabPageHelper();
                    var savefilehelper = new SaveFileHelper();

                    MenuFlyout myFlyout = new MenuFlyout();
                    MenuFlyoutItem CloseItem = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_Close/Text") };
                    MenuFlyoutItem LockFile = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_Lock/Text") };
                    MenuFlyoutItem ShareFile = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_Share/Text") };
                    MenuFlyoutItem FileInfo = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_Info/Text") };
                    MenuFlyoutItem SaveFile = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_Save/Text") };
                    MenuFlyoutItem CloseAll = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_CloseAll/Text") };
                    MenuFlyoutItem CloseAllButThis = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_CloseAll_ButThis/Text") };
                    MenuFlyoutItem CloseAllLeft = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_CloseAll_Left/Text") };
                    MenuFlyoutItem CloseAllRight = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_CloseAll_Right/Text") };
                    MenuFlyoutItem CloseAllWithoutSave = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_CloseAll_WithoutSave/Text") };
                    MenuFlyoutItem ExpandToSecondaryView = new MenuFlyoutItem { Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_OpenInNewWindow/Text") };
                    
                    if (ForLockedTab)
                    {
                        LockFile.Text = AppSettings.GetResourceStringStatic("TabRightClickMenu_Unlock/Text");
                    }

                    //Icons:
                    ShareFile.Icon = new SymbolIcon { Symbol = Symbol.Share };
                    LockFile.Icon = new SymbolIcon { Symbol = Symbol.ProtectedDocument };
                    SaveFile.Icon = new SymbolIcon { Symbol = Symbol.Save };
                    CloseItem.Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = "\uEF2C"
                    };
                    FileInfo.Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = "\uE946"
                    };
                    CloseAllLeft.Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = "\uEC52"
                    };
                    CloseAllRight.Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = "\uEBE7"
                    };
                    ExpandToSecondaryView.Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = "\uE78B"
                    };

                    LockFile.Click += delegate
                    {
                        tabpagehelper.SetUnsetLockFile(TabPage);
                    };
                    ShareFile.Click += delegate
                    {
                        new ShareFile(tabactions.GetTextBoxFromSelectedTabPage());
                    };
                    CloseItem.Click += async delegate
                    {
                        await tabactions.CloseTabAndSaveDataBase(TabPage);
                    };
                    SaveFile.Click += async delegate
                    {
                        await savefilehelper.Save(TabPage);
                    };
                    FileInfo.Click += async delegate
                    {
                        await new FileInfoDialog(TabPage).ShowAsync();
                    };
                    CloseAll.Click += async delegate
                    {
                        await tabactions.CloseAllTabs();
                    };
                    CloseAllButThis.Click += async delegate
                    {
                        await tabactions.CloseAllButThis(TabPage);
                    };
                    CloseAllLeft.Click += async delegate
                    {
                        await tabactions.CloseAllLeft(TabPage);
                    };
                    CloseAllRight.Click += async delegate
                    {
                        await tabactions.CloseAllRight(TabPage);
                    };
                    CloseAllWithoutSave.Click += async delegate
                    {
                        await tabactions.CloseAllWithoutSave();
                    };
                    ExpandToSecondaryView.Click += async delegate
                    {
                        await mainpage.secondaryviews.ExpandTabPageToNewView(TabPage);
                    };

                    myFlyout.Items.Add(CloseItem);
                    myFlyout.Items.Add(new MenuFlyoutSeparator());
                    myFlyout.Items.Add(FileInfo);
                    myFlyout.Items.Add(LockFile);
                    myFlyout.Items.Add(new MenuFlyoutSeparator());
                    myFlyout.Items.Add(ShareFile);
                    myFlyout.Items.Add(ExpandToSecondaryView);
                    myFlyout.Items.Add(new MenuFlyoutSeparator());
                    myFlyout.Items.Add(SaveFile);
                    myFlyout.Items.Add(new MenuFlyoutSeparator());
                    myFlyout.Items.Add(CloseAll);
                    myFlyout.Items.Add(CloseAllButThis);
                    myFlyout.Items.Add(CloseAllLeft);
                    myFlyout.Items.Add(CloseAllRight);
                    myFlyout.Items.Add(CloseAllWithoutSave);
                    return myFlyout;
                }
            }
            return null;
        }
    }
}
