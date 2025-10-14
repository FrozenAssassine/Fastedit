using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace Fastedit.Extensibility;

/// <summary>
/// Create a constructor without parameters and set values for all properties.
/// </summary>
public class CustomSyntaxHighlightLanguage : SyntaxHighlightLanguage, IExtensionInterface
{
    public CustomSyntaxHighlightLanguage() { }
}

public class CustomSyntaxHighlights : SyntaxHighlights
{
    public CustomSyntaxHighlights
        (string pattern, string colorLight, string colorDark, bool bold = false, bool italic = false, bool underlined = false)
        : base(pattern, colorLight, colorDark, bold, italic, underlined)
    {
    }
}