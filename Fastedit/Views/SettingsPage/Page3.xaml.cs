using Fastedit.Controls.Textbox;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace Fastedit.Views.SettingsPage
{
    public sealed partial class Page3 : Page
    {
        private AppSettings appsettings = new AppSettings();
        private TextControlBoxFlyoutMenu tcbflyoutmenu = new TextControlBoxFlyoutMenu(null);
        private List<(MenuFlyoutItem item, bool IsOn)> OutputList = new List<(MenuFlyoutItem, bool IsOn)>();
        List<MenuFlyoutItem> Originalbuttons = new List<MenuFlyoutItem>();

        public Page3()
        {
            this.InitializeComponent();

            Originalbuttons = tcbflyoutmenu.CreateButtons(null);
            RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), appsettings.GetSettingsAsString("ThemeIndex", "0"));

            //LoadAllItems();
        }

        /*private void LoadAllItems()
        {
            List<TextControlBoxFlyout_Button> AllItems = new List<TextControlBoxFlyout_Button>();
            List<string> checkeditems = tcbflyoutmenu.CreateListFromSettings();

            for (int i = 0; i < Originalbuttons.Count; i++)
            {
                AllItems.Add(new TextControlBoxFlyout_Button
                {
                    ButtonName = Originalbuttons[i].Name,
                    IsOn = false,
                    Text = Originalbuttons[i].Name,
                    CheckBoxName = Originalbuttons[i].Name + "_CB",
                });                    
            }

            List<TextControlBoxFlyout_Button> ItemsToAdd = new List<TextControlBoxFlyout_Button>();
            for (int i = 0; i < checkeditems.Count; i++)
            {
                //Check
                for (int j = 0; j < AllItems.Count; j++)
                {
                    if (AllItems[j] is TextControlBoxFlyout_Button tcbf_b)
                    {
                        if (tcbf_b.ButtonName == checkeditems[i])
                        {
                            ItemsToAdd.Add(tcbf_b);
                            break;
                        }
                    }
                }
            }
            for(int i = 0; i< ItemsToAdd.Count; i++)
            {
                AllItems.Remove(ItemsToAdd[i]);
            }
            for(int i = 0; i< AllItems.Count; i++)
            {
                TextControlBox_FlyoutItemSortingDisplay.Items.Add(AllItems[i]);
            }


        }

        //When pressing apply:
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveItems();
        }

        private async void SaveItems()
        {
            string outputtext = "";
            for (int i = 0; i<TextControlBox_FlyoutItemSortingDisplay.Items.Count; i++)
            {
                if(TextControlBox_FlyoutItemSortingDisplay.Items[i] is TextControlBoxFlyout_Button tcbf_b)
                {
                    if (tcbf_b.IsOn)
                    {
                        outputtext += tcbf_b.ButtonName + "|";
                    }
                }
            }
            appsettings.SaveSettings("TextControlBoxFlyout", outputtext);
            await new MessageDialog(outputtext, "").ShowAsync();
        }

        private void TextControlBox_FlyoutItem_Checked(object sender, RoutedEventArgs e)
        {
            if(sender is CheckBox cb)
            {
                for (int i = 0; i < TextControlBox_FlyoutItemSortingDisplay.Items.Count; i++)
                {
                    if (TextControlBox_FlyoutItemSortingDisplay.Items[i] is TextControlBoxFlyout_Button tcbf_b)
                    {
                        if (tcbf_b.CheckBoxName == cb.Name)
                        {
                            tcbf_b.IsOn = cb.IsChecked.HasValue && cb.IsChecked.Value;
                            break;
                        }
                    }
                }
            }
        }*/
    }

}
