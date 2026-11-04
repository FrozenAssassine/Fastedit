using Fastedit.Core.Settings;
using Fastedit.Models;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.UserProfile;
using WinUI3Localizer;

namespace Fastedit.Core;

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance => instance ??= new LocalizationManager();

    private ILocalizer localizer;
    private ResourceManager resourceManager;
    public ResourceContext resourceContext;
    public ResourceMap resourceMap;

    public LanguageItem CurrentLanguage { get; private set; }

    public List<LanguageItem> Languages { get; } = new();
    public List<LanguageItem> languages => Languages;

    public LocalizationManager()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        try
        {
            resourceManager ??= new ResourceManager();
            resourceContext ??= resourceManager.CreateResourceContext();
            resourceMap ??= resourceManager.MainResourceMap;
        }
        catch { }
    }

    public async Task InitializeLocalizerAsync()
    {
        EnsureInitialized();
        RegisterLanguageFromResource();

        var settingsLanguage = AppSettings.Language;
        LanguageItem res = null;

        if (!string.IsNullOrEmpty(settingsLanguage))
        {
            res = Languages.Find(x => x.Tag.Equals(settingsLanguage, StringComparison.OrdinalIgnoreCase));
        }

        if (res == null)
        {
            var systemLanguages = GlobalizationPreferences.Languages;
            string systemLanguage = systemLanguages.Count > 0 ? systemLanguages[0] : DefaultValues.DefaultLanguage;

            res = Languages.Find(x => systemLanguage.StartsWith(x.Tag, StringComparison.OrdinalIgnoreCase) || x.Tag.StartsWith(systemLanguage, StringComparison.OrdinalIgnoreCase));
            if (res == null)
                res = Languages.Find(x => x.Tag.Equals(DefaultValues.DefaultLanguage, StringComparison.OrdinalIgnoreCase)) ?? (Languages.Count > 0 ? Languages[0] : null);
        }

        string initialLanguage = res?.Tag ?? "en-US";

        try
        {
            string stringsFolderPath = Path.Combine(AppContext.BaseDirectory, "Strings");
            if (!Directory.Exists(stringsFolderPath))
            {
                stringsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Strings");
            }

            localizer = await new LocalizerBuilder()
                .AddStringResourcesFolderForLanguageDictionaries(stringsFolderPath)
                .SetOptions(options =>
                {
                    options.DefaultLanguage = initialLanguage;
                })
                .Build();
        }
        catch { }

        if (res != null)
        {
            await SetLanguageAsync(res);
        }
    }

    public async Task ReapplyLanguageAsync()
    {
        if (localizer != null && CurrentLanguage != null)
        {
            try
            {
                await localizer.SetLanguage(CurrentLanguage.Tag);
            }
            catch { }
        }
    }

    public void ReapplyLanguage()
    {
        _ = ReapplyLanguageAsync();
    }

    public void Initialize()
    {
        EnsureInitialized();
        RegisterLanguageFromResource();

        var settingsLanguage = AppSettings.Language;
        LanguageItem res = null;

        if (!string.IsNullOrEmpty(settingsLanguage))
        {
            res = Languages.Find(x => x.Tag.Equals(settingsLanguage, StringComparison.OrdinalIgnoreCase));
        }

        if (res == null)
        {
            var systemLanguages = GlobalizationPreferences.Languages;
            string systemLanguage = systemLanguages.Count > 0 ? systemLanguages[0] : DefaultValues.DefaultLanguage;

            res = Languages.Find(x => systemLanguage.StartsWith(x.Tag, StringComparison.OrdinalIgnoreCase) || x.Tag.StartsWith(systemLanguage, StringComparison.OrdinalIgnoreCase));
            if (res == null)
                res = Languages.Find(x => x.Tag.Equals(DefaultValues.DefaultLanguage, StringComparison.OrdinalIgnoreCase)) ?? (Languages.Count > 0 ? Languages[0] : null);
        }

        SetLanguage(res);
    }

    public async Task SetLanguageAsync(LanguageItem languageItem)
    {
        if (languageItem == null)
            return;

        CurrentLanguage = languageItem;

        try
        {
            ApplicationLanguages.PrimaryLanguageOverride = languageItem.Tag;
            EnsureInitialized();

            if (resourceContext != null)
            {
                resourceContext.QualifierValues["Language"] = languageItem.Tag;
            }

            if (localizer != null)
            {
                await localizer.SetLanguage(languageItem.Tag);
            }
        }
        catch { }

        AppSettings.Language = languageItem.Tag;
    }

    public void SetLanguage(LanguageItem languageItem)
    {
        _ = SetLanguageAsync(languageItem);
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        try
        {
            if (localizer != null)
            {
                var dict = localizer.GetCurrentLanguageDictionary();
                if (dict != null)
                {
                    string uid = key;
                    string prop = null;
                    if (key.Contains('.'))
                    {
                        var parts = key.Split('.', 2);
                        uid = parts[0];
                        prop = parts[1];
                    }

                    if (dict.TryGetItems(uid, out var items))
                    {
                        if (prop != null)
                        {
                            var match = items.FirstOrDefault(i => i.DependencyPropertyName.StartsWith(prop, StringComparison.OrdinalIgnoreCase));
                            if (match != null && !string.IsNullOrEmpty(match.Value))
                                return match.Value;
                        }
                        var first = items.FirstOrDefault();
                        if (first != null && !string.IsNullOrEmpty(first.Value))
                            return first.Value;
                    }
                }
            }

            EnsureInitialized();
            string normalized = key.Contains('.') ? key.Replace('.', '/') : key;

            var subtree = resourceMap?.GetSubtree("Resources");
            var candidate = subtree?.TryGetValue(normalized, resourceContext)
                         ?? subtree?.TryGetValue(key, resourceContext)
                         ?? resourceMap?.TryGetValue("Resources/" + normalized, resourceContext)
                         ?? resourceMap?.TryGetValue("Resources/" + key, resourceContext);

            if (candidate != null && !string.IsNullOrEmpty(candidate.ValueAsString))
                return candidate.ValueAsString;

            return key;
        }
        catch
        {
            return key;
        }
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
            Languages.Add(new LanguageItem("de-DE", "Deutsch"));
            Languages.Add(new LanguageItem("zh-CN", "简体中文"));
        }
    }
}
