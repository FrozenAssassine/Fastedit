using System.Linq;
using TextControlBoxNS;

namespace Fastedit.Core;

internal class SyntaxHighlightingManager
{
    public SyntaxHighlightLanguage[] GetAllSyntaxHighlights()
    {
        SyntaxHighlightLanguage[] inbuild = TextControlBox.SyntaxHighlightings.Select(item => item.Value).ToArray();
        SyntaxHighlightLanguage[] fromPlugins = [];
        SyntaxHighlightLanguage[] fromApp = [];

        return inbuild.Concat(fromPlugins).Concat(fromApp).ToArray();
    }

    public string GetIdentifier(SyntaxHighlightLanguage syntaxHighlighting)
    {
        return syntaxHighlighting.Author + syntaxHighlighting.Name;
    }
}
