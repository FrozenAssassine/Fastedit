using Fastedit.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Models;

internal class DummyAboutExtensionPage : IAboutPlugin
{
    public string PluginName => "Unknown name";

    public string PluginDescription => "Unknown description";

    public string PluginAuthor => "Unknown author";

    public string PluginAuthorURL => "";

    public Uri PluginIcon => new Uri("ms-appx:///Assets/AppIcon/Icon.png");
}
