using Fastedit.Core.Settings;
using Microsoft.UI.Windowing;
using System.Diagnostics;

internal class AppSettings
{
    public static bool DesignLoaded
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.DesignLoaded);
        set => SettingsManager.SaveSettings(AppSettingsValues.DesignLoaded, value);
    }

    public static bool FirstStart
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.App_FirstStart);
        set => SettingsManager.SaveSettings(AppSettingsValues.App_FirstStart, value);
    }

    public static string AppVersion
    {
        get => SettingsManager.GetSettings(AppSettingsValues.App_Version);
        set => SettingsManager.SaveSettings(AppSettingsValues.App_Version, value);
    }

    public static int FontSize
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.Settings_FontSize, DefaultValues.FontSize);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_FontSize, value);
    }

    public static string FontFamily
    {
        get => SettingsManager.GetSettings(AppSettingsValues.Settings_FontFamily, DefaultValues.FontFamily);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_FontFamily, value);
    }

    public static bool ShowLineHighlighter
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_ShowLineHighlighter, DefaultValues.ShowLineHighlighter);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_ShowLineHighlighter, value);
    }

    public static bool ShowLineNumbers
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_ShowLineNumbers, DefaultValues.ShowLinenumbers);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_ShowLineNumbers, value);
    }

    public static bool SyntaxHighlighting
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_Syntaxhighlighting, DefaultValues.SyntaxHighlighting);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_Syntaxhighlighting, value);
    }

    public static string NewTabTitle
    {
        get => SettingsManager.GetSettings(AppSettingsValues.Settings_NewTabTitle, DefaultValues.NewTabTitle);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_NewTabTitle, value);
    }

    public static string NewTabExtension
    {
        get => SettingsManager.GetSettings(AppSettingsValues.Settings_NewTabExtension, DefaultValues.NewTabExtension);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_NewTabExtension, value);
    }

    public static bool UseSpacesInsteadTabs
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_UseSpacesInsteadTabs, DefaultValues.UseSpacesInsteadTabs);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_UseSpacesInsteadTabs, value);
    }

    public static int SpacesPerTab
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.Settings_SpacesPerTab, DefaultValues.NumberOfSpacesPerTab);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_SpacesPerTab, value);
    }

    public static bool ShowStatusbar
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_ShowStatusbar, DefaultValues.ShowStatusbar);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_ShowStatusbar, value);
    }

    public static bool ShowMenubar
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_ShowMenubar, DefaultValues.ShowMenubar);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_ShowMenubar, value);
    }

    public static string Language
    {
        get => SettingsManager.GetSettings(AppSettingsValues.Settings_Language);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_Language, value);
    }

    public static string StatusbarSorting
    {
        get => SettingsManager.GetSettings(AppSettingsValues.Settings_StatusbarSorting, DefaultValues.StatusbarSorting);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_StatusbarSorting, value);
    }

    public static int TabViewWidthMode
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.Settings_TabViewWidthMode);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_TabViewWidthMode, value);
    }

    public static int MenubarAlignment
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.Settings_MenubarAlignment, DefaultValues.MenubarAlignment);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_MenubarAlignment, value);
    }

    public static string CurrentDesign
    {
        get => SettingsManager.GetSettings(AppSettingsValues.Settings_DesignName, DefaultValues.DefaultDesignName);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_DesignName, value);
    }
    public static bool HideTitlebar
    {
        get => SettingsManager.GetSettingsAsBool(AppSettingsValues.Settings_HideTitlebar, DefaultValues.HideTitlebar);
        set => SettingsManager.SaveSettings(AppSettingsValues.Settings_HideTitlebar, value);
    }

    public static int WindowWidth
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowWidth, DefaultValues.windowWidth);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowWidth, value);
    }
    public static int WindowHeight
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowHeight, DefaultValues.windowHeight);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowHeight, value);
    }
    public static int WindowLeft
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowLeft, DefaultValues.windowLeft);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowLeft, value);
    }
    public static int WindowTop
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowTop, DefaultValues.windowTop);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowTop, value);
    }
    public static OverlappedPresenterState WindowState
    {
        get => (OverlappedPresenterState)SettingsManager.GetSettingsAsInt(AppSettingsValues.windowState, 2);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowState, value.GetHashCode());
    }
}
