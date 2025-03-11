using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Extensibility;

/// <summary>
/// Implement this interface to give Ease Pass some information about your plugin. It is highly recommended to implement this interface a single time in every plugin.
/// </summary>
public interface IAboutPlugin : IExtensionInterface
{
    /// <summary>
    /// Name of your plugin
    /// </summary>
    string PluginName { get; }
    /// <summary>
    /// Description of your plugin
    /// </summary>
    string PluginDescription { get; }
    /// <summary>
    /// Author of your plugin
    /// </summary>
    string PluginAuthor { get; }
    /// <summary>
    /// URL to the authors webpage
    /// </summary>
    string PluginAuthorURL { get; }
    /// <summary>
    /// URI to the icon of the plugin
    /// </summary>
    Uri PluginIcon { get; }
}