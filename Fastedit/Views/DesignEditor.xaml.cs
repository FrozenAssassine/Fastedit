using Fastedit.Dialogs;
using Fastedit.Helper;
using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Fastedit.Core.Settings;
using Fastedit.Core.Tab;

namespace Fastedit.Views;

public sealed partial class DesignEditor : Page
{
    FasteditDesign currentDesign = null;

    public DesignEditor(FasteditDesign design, string designName)
    {
        this.InitializeComponent();
        this.currentDesign = design;
        this.CurrentDesignName = designName;

        this.titlebarHeader.Text = "Edit " + designName;
    }
    public string CurrentDesignName { get; private set; }
    public bool NeedSave { get; private set; } = false;
    public bool SaveDesign()
    {
        return DesignHelper.SaveDesign(currentDesign, Path.Combine(DefaultValues.DesignPath, CurrentDesignName));
    }
    //Change events:
    private void Theme_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb)
        {
            currentDesign.Theme = (ElementTheme)Enum.Parse(typeof(ElementTheme), cb.SelectedIndex.ToString());
            NeedSave = true;
        }
    }
    private void BackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.BackgroundColor = args.Color;
        NeedSave = true;
    }
    private void TextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.TextColor = args.Color;
        NeedSave = true;
    }
    private void SelectionColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.SelectionColor = args.Color;
        NeedSave = true;
    }
    private void LinenumberColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.LineNumberColor = args.Color;
        NeedSave = true;
    }
    private void LinenumberBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.LineNumberBackground = args.Color;
        NeedSave = true;
    }
    private void LinehighlighterColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.LineHighlighterBackground = args.Color;
        NeedSave = true;
    }
    private void TextboxBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.TextBoxBackground = args.Color;
        NeedSave = true;
    }
    private void TextboxBackgroundtype_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb)
        {
            currentDesign.TextboxBackgroundType = (ControlBackgroundType)Enum.Parse(typeof(ControlBackgroundType), cb.SelectedIndex.ToString());
            NeedSave = true;
        }
    }
    private void SearchHighlightColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.SearchHighlightColor = args.Color;
        NeedSave = true;
    }
    private void CursorColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.CursorColor = args.Color;
        NeedSave = true;
    }
    private void SelectedTabTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.SelectedTabPageHeaderTextColor = args.Color;
        NeedSave = true;
    }
    private void UnselectedTabTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.UnSelectedTabPageHeaderTextColor = args.Color;
        NeedSave = true;
    }
    private void SelectedTabBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.SelectedTabPageHeaderBackground = args.Color;
        NeedSave = true;
    }
    private void UnselectedTabBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.UnselectedTabPageHeaderBackground = args.Color;
        NeedSave = true;
    }
    private void StatusbarTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.StatusbarTextColor = args.Color;
        NeedSave = true;
    }
    private void StatusbarBackgroundType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb)
        {
            currentDesign.StatusbarBackgroundType = (ControlBackgroundType)Enum.Parse(typeof(ControlBackgroundType), cb.SelectedIndex.ToString());
            NeedSave = true;
        }
    }
    private void StatusbarBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.StatusbarBackgroundColor = args.Color;
        NeedSave = true;
    }
    private void DialogBackgroundType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb)
        {
            currentDesign.DialogBackgroundType = (ControlBackgroundType)Enum.Parse(typeof(ControlBackgroundType), cb.SelectedIndex.ToString());
            NeedSave = true;
        }
    }
    private void DialogBackgroundColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.DialogBackgroundColor = args.Color;
        NeedSave = true;
    }
    private void DialogTextColor_Changed(Microsoft.UI.Xaml.Controls.ColorPicker args)
    {
        currentDesign.DialogTextColor = args.Color;
        NeedSave = true;
    }

    private void BackgroundType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb)
        {
            currentDesign.BackgroundType = (BackgroundType)Enum.Parse(typeof(BackgroundType), cb.SelectedIndex.ToString());
            NeedSave = true;
        }
    }

    private void ApplyDesign_Click(object sender, RoutedEventArgs e)
    {
        //The design is already selected
        if (!AppSettings.CurrentDesign.Equals(CurrentDesignName))
        {
            AppSettings.CurrentDesign = CurrentDesignName;
        }

        DesignHelper.CurrentDesign = currentDesign;
        TabPageHelper.mainPage.ApplySettings(currentDesign);
    }

    private void SaveDesign_Click(object sender, RoutedEventArgs e)
    {
        if (SaveDesign())
            InfoMessages.SaveDesignSucceeded();
        else
            InfoMessages.SaveDesignError();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        NeedSave = false;
    }
}
