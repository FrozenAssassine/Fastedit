using Fastedit.Controls;
using Fastedit.Core.Tab;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Fastedit.Dialogs;

public class InfoMessages
{
    public static void NoAccessToSaveFile() => new InfoBar().Show("No access", "No access to write to the file", InfoBarSeverity.Error);
    public static void ErrorSavingDatabaseFile(string message) => new InfoBar().Show("Saving Database Error", message, InfoBarSeverity.Error);
    public static void ErrorSavingDBTempFile(string message) => new InfoBar().Show("Saving open file error", message, InfoBarSeverity.Error);
    public static void NoAccessToReadFile() => new InfoBar().Show("No access", "No access to read from the file", InfoBarSeverity.Error);
    public static void UnhandledException(string message) => new InfoBar().Show("Exception", "Unhandled exception:\n" + message, InfoBarSeverity.Error);
    public static void ClearRecycleBinError() => new InfoBar().Show("Clear Recycle Bin", "An error occurred while clearing the Recycle Bin", InfoBarSeverity.Error);
    public static void DeleteFromRecycleBinError() => new InfoBar().Show("Delete from Recycle Bin", "An error occurred while deleting the file from the Recycle Bin", InfoBarSeverity.Error);
    public static void RecycleBinClearSucceeded() => new InfoBar().Show("Clear Recycle Bin", "Successfully cleared the Recycle Bin", InfoBarSeverity.Success);
    public static void MoveToRecycleBinError() => new InfoBar().Show("Move to Recycle Bin", "An error occurred while moving the file to the Recycle Bin", InfoBarSeverity.Error);
    public static void OpenFromRecycleBinError() => new InfoBar().Show("Open from Recycle Bin", "An error occurred while opening the file from the Recycle Bin", InfoBarSeverity.Error);
    public static void FileNameInvalidCharacters() => new InfoBar().Show("Invalid Character", "The text entered contains invalid characters for a file name", InfoBarSeverity.Error);
    public static void DesignLoadError(string designName, Exception ex) => new InfoBar().Show("Design Load Failed", "Could not load the design: " + designName + "\n" + ex.Message, InfoBarSeverity.Error);
    public static void DetectEncodingError(Exception ex) => new InfoBar().Show("Detect Encoding", "Could not detect the encoding\n" + ex.Message, InfoBarSeverity.Error);
    public static void SettingsExportSucceeded() => new InfoBar().Show("Settings Export", "Settings successfully exported", InfoBarSeverity.Success);
    public static void SettingsImportSucceeded() => new InfoBar().Show("Settings Import", "Settings successfully imported", InfoBarSeverity.Success);
    public static void SettingsExportFailed() => new InfoBar().Show("Settings Export", "Failed to export settings", InfoBarSeverity.Error);
    public static void SettingsImportFailed() => new InfoBar().Show("Settings Import", "Failed to import settings", InfoBarSeverity.Error);
    public static void ClearTemporaryFilesFailed() => new InfoBar().Show("Temporary Files", "Failed to clear temporary files", InfoBarSeverity.Error);
    public static void ClearTemporaryFilesSucceeded() => new InfoBar().Show("Temporary Files", "Successfully cleared temporary files", InfoBarSeverity.Success);
    public static void DeleteDesignError(string design = "") => new InfoBar().Show("Delete Design", "Could not delete design" + (design.Length > 0 ? "\n" + design : ""), InfoBarSeverity.Error);
    public static void ImportDesignError() => new InfoBar().Show("Import Design", "Could not import design", InfoBarSeverity.Error);
    public static void ExportDesignError() => new InfoBar().Show("Export Design", "Could not export design", InfoBarSeverity.Error);
    public static void OneDesignMustRemain() => new InfoBar().Show("Could Not Delete", "Could not delete the design because at least one design must remain", InfoBarSeverity.Warning);
    public static void SaveDesignError() => new InfoBar().Show("Save Design", "Could not save the design", InfoBarSeverity.Error);
    public static void SaveDesignSucceeded() => new InfoBar().Show("Save Design", "The design was saved successfully", InfoBarSeverity.Success);
    public static void CloseDesignEditor() => new InfoBar().Show("Close Design Editor", "Please close all instances of the design editor", InfoBarSeverity.Warning);
    public static void RenameFileAlreadyExists() => new InfoBar().Show("Rename File", "Could not rename the file because a file with the same name already exists", InfoBarSeverity.Error);
    public static void RenameFileException(Exception ex) => new InfoBar().Show("Rename File", "An exception occurred while renaming the file:\n" + ex.Message, InfoBarSeverity.Error);
    public static void FileNotFoundReopenWithEncoding() => new InfoBar().Show("File Not Found", "Could not reopen the file because it no longer exists", InfoBarSeverity.Error);
    public static void InvalidDesignName() => new InfoBar().Show("Invalid Design Name", "The design name is invalid", InfoBarSeverity.Error);

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

        new InfoBar().Show(
            $"Welcome to Fastedit {version}!",
            "Updated to TextControlBox v1.5.1 due to bug in python syntaxhighlighting which caused crashes",
            btn,
            InfoBarSeverity.Success,
            25
        );
    }
}
