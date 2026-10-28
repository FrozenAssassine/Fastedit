using Fastedit.Core.Settings;
using Fastedit.Models;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using Windows.System.UserProfile;

namespace Fastedit.Core;

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance => instance ??= new LocalizationManager();

    private ResourceManager resourceManager = new();
    public ResourceContext resourceContext;
    public ResourceMap resourceMap = null;

    public List<LanguageItem> Languages { get; } = new();
    public List<LanguageItem> languages => Languages;

    public LocalizationManager()
    {
        try
        {
            resourceContext = resourceManager.CreateResourceContext();
            resourceMap = resourceManager.MainResourceMap;
        }
        catch { }
    }

    public void SetLanguage(LanguageItem languageItem)
    {
        if (languageItem == null)
            return;

        if (Languages.Contains(languageItem))
        {
            try
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageItem.Tag;
            }
            catch { }

            AppSettings.Language = languageItem.Tag;
        }
    }

    public void Initialize()
    {
        RegisterLanguageFromResource();

        var systemLanguages = GlobalizationPreferences.Languages;
        string systemLanguage = systemLanguages.Count > 0 ? systemLanguages[0] : DefaultValues.DefaultLanguage;

        var settingsLanguage = AppSettings.Language;
        var res = Languages.Find(x => x.Tag.Equals(settingsLanguage, StringComparison.OrdinalIgnoreCase));
        if (res == null)
        {
            res = Languages.Find(x => systemLanguage.StartsWith(x.Tag, StringComparison.OrdinalIgnoreCase) || x.Tag.StartsWith(systemLanguage, StringComparison.OrdinalIgnoreCase));
            if (res == null)
                res = Languages.Find(x => x.Tag.Equals(DefaultValues.DefaultLanguage, StringComparison.OrdinalIgnoreCase)) ?? (Languages.Count > 0 ? Languages[0] : null);

            SetLanguage(res);
            return;
        }

        SetLanguage(res);
    }

    private void RegisterLanguageFromResource()
    {
        Languages.Clear();

        try
        {
            ResourceMap subtree = resourceManager?.MainResourceMap?.GetSubtree("LanguageList");
            if (subtree != null)
            {
                uint length = subtree.ResourceCount;
                for (uint i = 0; i < length; i++)
                {
                    var resource = subtree.GetValueByIndex(i);
                    Languages.Add(new LanguageItem(resource.Key, resource.Value.ValueAsString));
                }
            }
        }
        catch { }

        if (Languages.Count == 0)
        {
            Languages.Add(new LanguageItem("en-US", "English"));
            Languages.Add(new LanguageItem("zh-CN", "简体中文"));
        }
    }
}
