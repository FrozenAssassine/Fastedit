using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Extensibility;

public interface IExtensionSource : IExtensionInterface
{
    /// <summary>
    /// Name of the extension source.
    /// </summary>
    string SourceName { get; }
    /// <summary>
    /// Ease Pass will call this function to get the extension sources.
    /// </summary>
    /// <returns>The extension source and name.</returns>
    (Uri Source, string Name)[] GetExtensionSources();
}