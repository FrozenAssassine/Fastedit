using Fastedit.Core.Tab;
using Fastedit.Dialogs;
using Fastedit.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Helper
{
    public class SecondaryView
    {
        public List<OpenedSecondaryViewItem> OpenedSecondaryViews = new List<OpenedSecondaryViewItem>();
        private TabActions tabactions;
        private muxc.TabView TextTabControl;
        private MainPage mainpage;
        private TabPageHelper tabpagehelper = new TabPageHelper();

        public SecondaryView(MainPage mp, TabActions tabactions, muxc.TabView tabview)
        {
            this.mainpage = mp;
            this.tabactions = tabactions;
            this.TextTabControl = tabview;
        }

        private async Task<int> GetViewIndexFromAppView(int ViewId)
        {
            int returnval = -1;
            for (int i = 0; i < OpenedSecondaryViews.Count; i++)
            {
                await OpenedSecondaryViews[i].CoreApplicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (ApplicationView.GetForCurrentView().Id == ViewId)
                    {
                        returnval = i;
                    }
                });
            }
            return returnval;
        }
        public async Task ExpandTabPageToNewView(muxc.TabViewItem TabPage)
        {
            var textcontrolbox = tabactions.GetTextBoxFromTabPage(TabPage);

            //Run code only if tabpage is a textbox:
            if (textcontrolbox == null) return;

            string TabPageName = TabPage.Name;
            string Header = textcontrolbox.Header;
            string tbtext = textcontrolbox.GetText();
            double tbZoomFactor = textcontrolbox._zoomFactor;
            TextWrapping wrapping = textcontrolbox.WordWrap;
            bool IsReadOnly = textcontrolbox.IsReadOnly;
            int SelectionStart = textcontrolbox.SelectionStart;
            ApplicationView appview = null;
            CoreApplicationView newView = CoreApplication.CreateNewView();

            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(TextboxViewPage));
                Window.Current.Content = frame;
                Window.Current.Activate();

                if (frame.Content is TextboxViewPage tbviewpage)
                {
                    tbviewpage.TabPageName = TabPageName;
                    tbviewpage.Title = Header;
                    tbviewpage.TextboxText = tbtext;
                    tbviewpage.SelectionStart = SelectionStart;
                    tbviewpage.Textwrapping = wrapping;
                    tbviewpage.ZoomFactor = tbZoomFactor;
                    tbviewpage.IsReadonly = IsReadOnly;
                }

                appview = ApplicationView.GetForCurrentView();
                newViewId = appview.Id;
                appview.Consolidated += TextboxViewPage_Consolidated;
            });

            TabPage.Visibility = Visibility.Collapsed;

            //if all tabs are in a secondary view, create a new tab
            if (tabactions.GetShownTabPages() < 1)
            {
                muxc.TabViewItem tab = tabactions.NewTab();
                if (tab != null)
                {
                    TextTabControl.SelectedItem = tab;
                    await tabactions.SaveAllTabChanges();
                    mainpage.SetSettingsToTabPage(tab, mainpage.TextBoxMargin());
                }
            }

            await tabactions.GetTextBoxFromTabPage(TabPage).SetText("Cleared the textbox");
            for (int i = 0; i < TextTabControl.TabItems.Count; i++)
            {
                if (TextTabControl.TabItems[i] is muxc.TabViewItem Tab)
                {
                    if (Tab.Visibility == Visibility.Visible)
                    {
                        TextTabControl.SelectedItem = Tab;
                        break;
                    }
                }
            }

            if (!await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId))
            {
                mainpage.ShowInfobar(InfoBarMessages.CouldNotOpenInNewView, InfoBarMessages.CouldNotOpenInNewViewTitle, muxc.InfoBarSeverity.Error);
                return;
            }
            OpenedSecondaryViews.Add(new OpenedSecondaryViewItem
            {
                ApplicationView = appview,
                CoreApplicationView = newView
            });
        }
        private async Task<bool> CloseExpandedView(ApplicationView sender, bool RemoveFromOpenedViewList = true)
        {
            int index = await GetViewIndexFromAppView(sender.Id);
            if (index < 0 || index >= OpenedSecondaryViews.Count)
                return false;

            TextboxViewPage tbvpage = null;
            string TabPageName = "";

            //Get the view by the index returned from the GetViewIndexFromAppView
            var view = OpenedSecondaryViews[index];
            await view.CoreApplicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    if (frame.Content is TextboxViewPage tbvp)
                    {
                        tbvpage = tbvp;
                        TabPageName = tbvpage.TabPageName;
                    }
                }
            });
            if (tbvpage == null)
                return false;
            double ZoomFactor = tbvpage.ZoomFactor;
            bool IsReadOnly = tbvpage.IsReadonly;
            string TextBoxText = tbvpage.TextboxText;
            int SelectionStart = tbvpage.SelectionStart;
            bool IsModified = tbvpage.IsModified;

            if (tbvpage != null)
            {
                await mainpage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (TextTabControl.FindName(TabPageName) is muxc.TabViewItem tabpage)
                    {
                        var textbox = tabactions.GetTextBoxFromTabPage(tabpage);
                        if (textbox != null)
                        {
                            await textbox.SetText(TextBoxText);
                            textbox.SetFontZoomFactor(ZoomFactor);
                            textbox.IsReadOnly = IsReadOnly;
                            textbox.SelectionStart = SelectionStart;
                            tabpagehelper.SetTabModified(tabpage, IsModified);
                        }
                        tabpage.Visibility = Visibility.Visible;
                        TextTabControl.SelectedItem = tabpage;
                    }
                });
                if (RemoveFromOpenedViewList)
                    return OpenedSecondaryViews.Remove(view);
                return true;
            }
            return false;
        }
        private async void TextboxViewPage_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            await CloseExpandedView(sender);
        }

    }
}