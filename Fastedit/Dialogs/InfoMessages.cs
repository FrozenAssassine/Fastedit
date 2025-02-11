using Fastedit.Controls;
using Fastedit.Core.Tab;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Fastedit.Dialogs;

public class InfoMessages
{
    public static void NoAccesToSaveFile() => new InfoBar().Show("No access", "No access to write to file", InfoBarSeverity.Error);
    public static void NoAccesToReadFile() => new InfoBar().Show("No access", "No access to read from file", InfoBarSeverity.Error);
    public static void UnhandledException(string message) => new InfoBar().Show("Exception", "Unhandled exception: \n" + message, InfoBarSeverity.Error);
    public static void ClearRecyclebinError() => new InfoBar().Show("Clear recyclebin", "Error occured while clearing the recycle bin", InfoBarSeverity.Error);
    public static void DeleteFromRecyclebinError() => new InfoBar().Show("Delete from recyclebin", "Error occured while deleting the file from recycle bin", InfoBarSeverity.Error);
    public static void RecyclebinClearSucceed() => new InfoBar().Show("Clear recyclebin", "Successfully cleared recycle bin", InfoBarSeverity.Success);
    public static void MoveToRecyclebinError() => new InfoBar().Show("Move to recyclebin", "Error occured while moving the file to recyclebin", InfoBarSeverity.Error);
    public static void OpenFromRecyclebinError() => new InfoBar().Show("Open from recyclebin", "Error occured while opening the file from the recyclebin", InfoBarSeverity.Error);
    public static void FileNameInvalidCharacters() => new InfoBar().Show("Invalid character", "The text entered contains invalid characters for a file name", InfoBarSeverity.Error);
    public static void DesignLoadError(string designName, Exception ex) => new InfoBar().Show("Design load failed", "Could not load the design " + designName + "\n" + ex.Message, InfoBarSeverity.Error);
    public static void DetectEncodingError(Exception ex) => new InfoBar().Show("Detect encoding", "Could not detect the encoding\n" + ex.Message, InfoBarSeverity.Error);
    public static void SettingsExportSucceed() => new InfoBar().Show("Settings export", "Settings successfully exported", InfoBarSeverity.Success);
    public static void SettingsImportSucceed() => new InfoBar().Show("Settings import", "Settings successfully imported", InfoBarSeverity.Success);
    public static void SettingsExportFailed() => new InfoBar().Show("Settings export", "Failed while exporting settings", InfoBarSeverity.Error);
    public static void SettingsImportFailed() => new InfoBar().Show("Settings import", "Faild while importing settings", InfoBarSeverity.Error);
    public static void ClearTemporaryFilesFailed() => new InfoBar().Show("Temporary files", "Failed while clearing temporary files", InfoBarSeverity.Error);
    public static void ClearTemporaryFilesSucceed() => new InfoBar().Show("Temporary files", "Successfully cleared temporary files", InfoBarSeverity.Success);
    public static void DeleteDesignError(string design = "") => new InfoBar().Show("Delete design", "Could not delete design" + (design.Length > 0 ? "\n" + design : ""), InfoBarSeverity.Error);
    public static void ImportDesignError() => new InfoBar().Show("Import design", "Could not import design", InfoBarSeverity.Error);
    public static void ExportDesignError() => new InfoBar().Show("Import design", "Could not export design", InfoBarSeverity.Error);
    public static void OneDesignNeedsToBeLeft() => new InfoBar().Show("Could not delete", "Could not delete the design, because there always has to be at least one", InfoBarSeverity.Warning);
    public static void SaveDesignError() => new InfoBar().Show("Save design", "Could not save the design", InfoBarSeverity.Error);
    public static void SaveDesignSucceed() => new InfoBar().Show("Save design", "The design was saved successfully", InfoBarSeverity.Success);
    public static void CloseDesignEditor() => new InfoBar().Show("Close design editor", "Please close all instances of the design editor", InfoBarSeverity.Warning);
    public static void RenameFileAlreadyExists() => new InfoBar().Show("Rename file", "Could not rename file, because a file with the same name already exists.", InfoBarSeverity.Error);
    public static void RenameFileException(Exception ex) => new InfoBar().Show("Rename file", "Exception while renaming file:\n" + ex.Message, InfoBarSeverity.Error);
    public static void FileNotFoundReopenWithEncoding() => new InfoBar().Show("File not found", "Could not reopen the file, because it does not exist anymore.", InfoBarSeverity.Error);
    public static void InvalidDesignName() => new InfoBar().Show("Invalid Design name", "The Design name is invalid", InfoBarSeverity.Error);

    public static void WelcomeMessage()
    {
        var btn = new Button { Content = "Designs" };
        btn.Click += delegate
        {
            TabPageHelper.mainPage.ShowSettings("DesignPage");
        };
        new InfoBar().Show("Welcome to Fastedit", "To customize your experience,\npress the button below", btn, InfoBarSeverity.Success, 8);
    }

    public static void NewVersionInfo(string version)
    {
        var btn = new Button { Content = "Changelog" };
        btn.Click += delegate
        {
            TabPageHelper.mainPage.ShowSettings("AboutPage");
        };

        new InfoBar().Show("New version", "Welcome to Fastedit version " + version + "\n", btn, InfoBarSeverity.Success, 8);
    }
}
