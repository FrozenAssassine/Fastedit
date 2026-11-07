namespace Fastedit.Models;

public class LanguageItem
{
    public string Tag { get; set; }
    public string DisplayName { get; set; }

    public LanguageItem(string tag, string displayName)
    {
        Tag = tag;
        DisplayName = displayName;
    }
}
