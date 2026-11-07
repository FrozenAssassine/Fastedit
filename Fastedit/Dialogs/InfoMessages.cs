using Fastedit.Controls;
using Fastedit.Core.Tab;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Fastedit.Dialogs;

public class InfoMessages
{
    public static void NoAccessToSaveFile() => new InfoBar().Show(Multilingual.L("T_NoAccess"), Multilingual.L("T_NoAccessToWriteToTheFile"), InfoBarSeverity.Error);
    public static void ErrorSavingDatabaseFile(string message) => new InfoBar().Show(Multilingual.L("T_SavingDatabaseError"), message, InfoBarSeverity.Error);
    public static void ErrorSavingDBTempFile(string message) => new InfoBar().Show(Multilingual.L("T_SavingOpenFileError"), message, InfoBarSeverity.Error);
    public static void NoAccessToReadFile() => new InfoBar().Show(Multilingual.L("T_NoAccess"), Multilingual.L("T_NoAccessToReadFromTheFile"), InfoBarSeverity.Error);
    public static void UnhandledException(string message) => new InfoBar().Show(Multilingual.L("T_Exception"), Multilingual.L("T_UnhandledExceptionN") + message, InfoBarSeverity.Error);
    public static void ClearRecycleBinError() => new InfoBar().Show(Multilingual.L("T_ClearRecycleBin"), Multilingual.L("T_AnErrorOccurredWhileClearingTheRecycleBin"), InfoBarSeverity.Error);
    public static void DeleteFromRecycleBinError() => new InfoBar().Show(Multilingual.L("T_DeleteFromRecycleBin"), Multilingual.L("T_AnErrorOccurredWhileDeletingTheFileFromTheRecycleBin"), InfoBarSeverity.Error);
    public static void RecycleBinClearSucceeded() => new InfoBar().Show(Multilingual.L("T_ClearRecycleBin"), Multilingual.L("T_SuccessfullyClearedTheRecycleBin"), InfoBarSeverity.Success);
    public static void MoveToRecycleBinError() => new InfoBar().Show(Multilingual.L("T_MoveToRecycleBin"), Multilingual.L("T_AnErrorOccurredWhileMovingTheFileToTheRecycleBin"), InfoBarSeverity.Error);
    public static void OpenFromRecycleBinError() => new InfoBar().Show(Multilingual.L("T_OpenFromRecycleBin"), Multilingual.L("T_AnErrorOccurredWhileOpeningTheFileFromTheRecycleBin"), InfoBarSeverity.Error);
    public static void FileNameInvalidCharacters() => new InfoBar().Show(Multilingual.L("T_InvalidCharacter"), Multilingual.L("T_TheTextEnteredContainsInvalidCharactersForAFileName"), InfoBarSeverity.Error);
    public static void DesignLoadError(string designName, Exception ex) => new InfoBar().Show(Multilingual.L("T_DesignLoadFailed"), Multilingual.L("T_CouldNotLoadTheDesign") + designName + "\n" + ex.Message, InfoBarSeverity.Error);
    public static void DetectEncodingError(Exception ex) => new InfoBar().Show(Multilingual.L("T_DetectEncoding"), Multilingual.L("T_CouldNotDetectTheEncodingN") + ex.Message, InfoBarSeverity.Error);
    public static void SettingsExportSucceeded() => new InfoBar().Show(Multilingual.L("T_SettingsExport"), Multilingual.L("T_SettingsSuccessfullyExported"), InfoBarSeverity.Success);
    public static void SettingsImportSucceeded() => new InfoBar().Show(Multilingual.L("T_SettingsImport"), Multilingual.L("T_SettingsSuccessfullyImported"), InfoBarSeverity.Success);
    public static void SettingsExportFailed() => new InfoBar().Show(Multilingual.L("T_SettingsExport"), Multilingual.L("T_FailedToExportSettings"), InfoBarSeverity.Error);
    public static void SettingsImportFailed() => new InfoBar().Show(Multilingual.L("T_SettingsImport"), Multilingual.L("T_FailedToImportSettings"), InfoBarSeverity.Error);
    public static void ClearTemporaryFilesFailed() => new InfoBar().Show(Multilingual.L("T_TemporaryFiles"), Multilingual.L("T_FailedToClearTemporaryFiles"), InfoBarSeverity.Error);
    public static void ClearTemporaryFilesSucceeded() => new InfoBar().Show(Multilingual.L("T_TemporaryFiles"), Multilingual.L("T_SuccessfullyClearedTemporaryFiles"), InfoBarSeverity.Success);
    public static void DeleteDesignError(string design = "") => new InfoBar().Show(Multilingual.L("T_DeleteDesign"), Multilingual.L("T_CouldNotDeleteDesign") + (design.Length > 0 ? "\n" + design : ""), InfoBarSeverity.Error);
    public static void ImportDesignError() => new InfoBar().Show(Multilingual.L("T_ImportDesign"), Multilingual.L("T_CouldNotImportDesign"), InfoBarSeverity.Error);
    public static void ExportDesignError() => new InfoBar().Show(Multilingual.L("T_ExportDesign"), Multilingual.L("T_CouldNotExportDesign"), InfoBarSeverity.Error);
    public static void OneDesignMustRemain() => new InfoBar().Show(Multilingual.L("T_CouldNotDelete"), Multilingual.L("T_CouldNotDeleteTheDesignBecauseAtLeastOneDesignMustRemain"), InfoBarSeverity.Warning);
    public static void SaveDesignError() => new InfoBar().Show(Multilingual.L("T_SaveDesign_2"), Multilingual.L("T_CouldNotSaveTheDesign"), InfoBarSeverity.Error);
    public static void SaveDesignSucceeded() => new InfoBar().Show(Multilingual.L("T_SaveDesign_2"), Multilingual.L("T_TheDesignWasSavedSuccessfully"), InfoBarSeverity.Success);
    public static void CloseDesignEditor() => new InfoBar().Show(Multilingual.L("T_CloseDesignEditor"), Multilingual.L("T_PleaseCloseAllInstancesOfTheDesignEditor"), InfoBarSeverity.Warning);
    public static void RenameFileAlreadyExists() => new InfoBar().Show(Multilingual.L("T_RenameFile"), Multilingual.L("T_CouldNotRenameTheFileBecauseAFileWithTheSameNameAlreadyExist"), InfoBarSeverity.Error);
    public static void RenameFileException(Exception ex) => new InfoBar().Show(Multilingual.L("T_RenameFile"), Multilingual.L("T_AnExceptionOccurredWhileRenamingTheFileN") + ex.Message, InfoBarSeverity.Error);
    public static void FileNotFoundReopenWithEncoding() => new InfoBar().Show(Multilingual.L("T_FileNotFound"), Multilingual.L("T_CouldNotReopenTheFileBecauseItNoLongerExists"), InfoBarSeverity.Error);
    public static void InvalidDesignName() => new InfoBar().Show(Multilingual.L("T_InvalidDesignName"), Multilingual.L("T_TheDesignNameIsInvalid"), InfoBarSeverity.Error);

    public static void WelcomeMessage()
    {
        var btn = new Button { Content = Multilingual.L("T_Designs") };
        btn.Click += delegate
        {
            TabPageHelper.mainPage.ShowSettings("DesignPage");
        };
        new InfoBar().Show(Multilingual.L("T_WelcomeToFastedit"), Multilingual.L("T_ToCustomizeYourExperienceNpressTheButtonBelow"), btn, InfoBarSeverity.Success, 8);
    }

    public static void NewVersionInfo(string version)
    {
        var btn = new Button { Content = Multilingual.L("T_Changelog") };
        btn.Click += delegate
        {
            TabPageHelper.mainPage.ShowSettings("AboutPage");
        };

        new InfoBar().Show(
            string.Format(Multilingual.L("T_WelcomeToFasteditVersion"), version),
            "• Added multi-language support (English, German, Simplified Chinese)\n• Added language switcher in App settings with live reload",
            btn,
            InfoBarSeverity.Success,
            25
        );
    }
}
