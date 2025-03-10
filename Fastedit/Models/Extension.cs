using Fastedit.Extensibility;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Models;

internal class Extension
{
    public IExtensionInterface[] Interfaces;

    public IAboutPlugin AboutPlugin = null;

    public ImageSource IconSource
    {
        get
        {
            if (AboutPlugin != null) return new BitmapImage(AboutPlugin.PluginIcon);
            return null;
        }
    }

    public string ID = "";

    public Extension(IExtensionInterface[] interfaces, string id)
    {
        Interfaces = interfaces;
        int length = interfaces.Length;
        for (int i = 0; i < length; i++)
        {
            if (interfaces[i] is IAboutPlugin plugin)
            {
                AboutPlugin = plugin;
            }
        }
        if (AboutPlugin == null)
        {
            AboutPlugin = new DummyAboutExtensionPage();
        }
        ID = id;
    }

    public override string ToString()
    {
        return ToString(true);
    }

    public string ToString(bool headline)
    {
        List<string> items = new List<string>();
        int length = Interfaces.Length;
        for (int i = 0; i < length; i++)
        {
            if (Interfaces[i] is CustomSyntaxHighlightLanguage) items.Add("• add custom syntax highlighting");
            if (Interfaces[i] is IExtensionSource) items.Add("• add extensions to the store");
            // fill up with other interfaces
        }
        List<string> itemsFinal = new List<string>();
        for (int i = 0; i < items.Count; i++)
        {
            if (!itemsFinal.Contains(items[i]))
                itemsFinal.Add(items[i]);
        }

        StringBuilder sb = new StringBuilder();
        if (headline)
        {
            sb.AppendLine("Authorizations requested by plugin:");
        }

        for (int i = 0; i < itemsFinal.Count; i++)
        {
            sb.AppendLine(itemsFinal[i]);
        }

        return sb.ToString();
    }
}
