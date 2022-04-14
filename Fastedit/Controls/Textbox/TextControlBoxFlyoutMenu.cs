using Fastedit.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace Fastedit.Controls.Textbox
{
    public class TextControlBoxFlyoutMenu
    {
        private List<MenuFlyoutItem> buttons = null;
        private AppSettings appsettings = new AppSettings();
        private TextControlBox textbox = null;

        public TextControlBoxFlyoutMenu(TextControlBox textbox)
        {
            buttons = CreateButtons(textbox);
            this.textbox = textbox;
        }

        /*
        //Retrive from settings and create the flyout
        public List<string> CreateListFromSettings()
        {
            try
            {
                List<string> Items = new List<string>();

                string Buttons = appsettings.GetSettingsAsString("TextControlBoxFlyout", "");
                if (Buttons.Length == 0)
                    return null;

                var split = Buttons.Split("|", StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < split.Length; j++)
                {
                    Items.Add(split[j]);
                }
                return Items;
            }
            catch(Exception e)
            {
                Debug.WriteLine("CreateListFromSettings : " + e.Message);
                return null;
            }
        }
        public MenuFlyout CreateFlyoutFromList(List<string> items)
        {
            try
            {
                var flyout = new MenuFlyout();
                flyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft;
                flyout.ShowMode = Windows.UI.Xaml.Controls.Primitives.FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
                if (items == null)
                    return null;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Contains("Separator", System.StringComparison.Ordinal))
                    {
                        flyout.Items.Add(new MenuFlyoutSeparator());
                    }
                    else
                    {
                        flyout.Items.Add(GetButtonFromList(items[i]));
                    }
                }
                return flyout;
            }
            catch (Exception e)
            {
                Debug.WriteLine("CreateFlyoutFromList : " + e.Message);
                return null;
            }
        }
        */
        public List<MenuFlyoutItem> CreateButtons(TextControlBox textbox)
        {
            List<MenuFlyoutItem> lst = new List<MenuFlyoutItem>();
            string CopyText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Copy/Text");
            string PasteText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Paste/Text");
            string CutText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Cut/Text");
            string UndoText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Undo/Text");
            string SelectAllText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_SelectAll/Text");
            var CopyBtn = new MenuFlyoutItem { Name = "Copy", Text = CopyText, Icon = new SymbolIcon { Symbol = Symbol.Copy } };
            var CutBtn = new MenuFlyoutItem { Name = "Cut", Text = CutText, Icon = new SymbolIcon { Symbol = Symbol.Cut } };
            var FindText = new MenuFlyoutItem { Name = "Find", Text = "Find", Icon = new SymbolIcon { Symbol = Symbol.Find } };
            var ShareText = new MenuFlyoutItem { Name = "Share", Text = "Share selected", Icon = new SymbolIcon { Symbol = Symbol.Share } };
            var PasteBtn = new MenuFlyoutItem { Name = "Paste", Text = PasteText, Icon = new SymbolIcon { Symbol = Symbol.Paste } };
            var UndoBtn = new MenuFlyoutItem { Name = "Undo", Text = UndoText, Icon = new SymbolIcon { Symbol = Symbol.Undo } };
            var Selectall = new MenuFlyoutItem { Name = "SelectAll", Text = SelectAllText, Icon = new SymbolIcon { Symbol = Symbol.SelectAll } };
            ToolTipService.SetToolTip(CopyBtn, CopyText);
            ToolTipService.SetToolTip(PasteBtn, PasteText);
            ToolTipService.SetToolTip(CutBtn, CutText);
            ToolTipService.SetToolTip(UndoBtn, UndoText);
            ToolTipService.SetToolTip(Selectall, SelectAllText);
            ToolTipService.SetToolTip(ShareText, "Share the selected text");
            lst.Add(CopyBtn);
            lst.Add(PasteBtn);
            lst.Add(CutBtn);
            lst.Add(UndoBtn);
            lst.Add(Selectall);
            lst.Add(ShareText);
            lst.Add(FindText);
            //Only create the buttons, without the events:
            //if (textbox == null)
            //    return lst;

            CopyBtn.Click += delegate
            {
                textbox.Copy();
            };
            PasteBtn.Click += delegate
            {
                textbox.Paste();
            };
            CutBtn.Click += delegate
            {
                textbox.Cut();
            };
            UndoBtn.Click += delegate
            {
                textbox.Undo();
            };
            Selectall.Click += delegate
            {
                textbox.SelectAll();
            };
            ShareText.Click += delegate
            {
                ShareFile.ShareText(textbox.SelectedText);
            };
            FindText.Click += delegate
            {
                textbox.FindInText(textbox.SelectedText, false, false, false);
            };
            return lst;
        }
        public MenuFlyoutItem GetButtonFromList(string name)
        {
            return buttons.Find(item => item.Name == name);
        }

        public MenuFlyout CreateFlyout(bool SelectionFlyout)
        {
            if (textbox == null)
                return null;

            if (buttons == null)
                buttons = CreateButtons(textbox);
            var flyout = new MenuFlyout();
            flyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft;
            flyout.ShowMode = Windows.UI.Xaml.Controls.Primitives.FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
            //if text is selected
            if (SelectionFlyout)
            {
                flyout.Items.Add(GetButtonFromList("Copy"));
                flyout.Items.Add(GetButtonFromList("Cut"));
                flyout.Items.Add(GetButtonFromList("Paste"));
                flyout.Items.Add(new MenuFlyoutSeparator());
                flyout.Items.Add(GetButtonFromList("Find"));
                flyout.Items.Add(GetButtonFromList("Share"));
                flyout.Items.Add(new MenuFlyoutSeparator());
                flyout.Items.Add(GetButtonFromList("Undo"));
                flyout.Items.Add(GetButtonFromList("SelectAll"));
            }
            else //if no text is selected
            {
                flyout.Items.Add(GetButtonFromList("Copy"));
                flyout.Items.Add(GetButtonFromList("Cut"));
                flyout.Items.Add(GetButtonFromList("Paste"));
                flyout.Items.Add(new MenuFlyoutSeparator());
                flyout.Items.Add(GetButtonFromList("Undo"));
                flyout.Items.Add(GetButtonFromList("SelectAll"));
            }
            return flyout;
            //return CreateFlyoutFromList(CreateListFromSettings());
        }

        public CommandBarFlyout CreateSelectionFlyout()
        {
            if (textbox == null)
                return null;

            if (buttons == null)
                buttons = CreateButtons(textbox);

            string CopyText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Copy/Text");
            string PasteText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Paste/Text");
            string CutText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Cut/Text");
            string UndoText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_Undo/Text");
            string SelectAllText = AppSettings.GetResourceStringStatic("TextControlBoxFlyoutMenu_SelectAll/Text");
            var CopyBtn = new AppBarButton { Name = "Copy", Label = CopyText, Icon = new SymbolIcon { Symbol = Symbol.Copy }, LabelPosition = CommandBarLabelPosition.Collapsed };
            var CutBtn = new AppBarButton { Name = "Cut", Label = CutText, Icon = new SymbolIcon { Symbol = Symbol.Cut }, LabelPosition = CommandBarLabelPosition.Collapsed };
            var FindText = new AppBarButton { Name = "Find", Label = "Find", Icon = new SymbolIcon { Symbol = Symbol.Find }, LabelPosition = CommandBarLabelPosition.Collapsed };
            var ShareText = new AppBarButton { Name = "Share", Label = "Share selected", Icon = new SymbolIcon { Symbol = Symbol.Share }, LabelPosition =CommandBarLabelPosition.Collapsed };
            ToolTipService.SetToolTip(CopyBtn, CopyText);
            ToolTipService.SetToolTip(CutBtn, CutText);
            ToolTipService.SetToolTip(ShareText, "Share selected");
            ToolTipService.SetToolTip(FindText, "Find selected");

            ShareText.Click += delegate
            {
                ShareFile.ShareText(textbox.SelectedText);
            };
            FindText.Click += delegate
            {
                textbox.FindInText(textbox.SelectedText, false, false, false);
            };
            CutBtn.Click += delegate
            {
                textbox.Cut();
            };
            CopyBtn.Click += delegate
            {
                textbox.Copy();
            };
            var flyout = new CommandBarFlyout();
            flyout.PrimaryCommands.Add(CopyBtn);
            flyout.PrimaryCommands.Add(CutBtn);
            flyout.PrimaryCommands.Add(ShareText);
            flyout.PrimaryCommands.Add(FindText);
            return flyout;
        }
    }
    //public class TextControlBoxFlyout_Button
    //{
    //    public string ButtonName { get; set; }
    //    public string Text { get; set; } = "";
    //    public bool IsOn { get; set; } = false;
    //    public string CheckBoxName { get; set; } = "";
    //}
}
