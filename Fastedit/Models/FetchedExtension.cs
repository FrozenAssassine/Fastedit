using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Models;

internal class FetchedExtension
{
    public Uri URL { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }

    public FetchedExtension(Uri url, string name, string source)
    {
        this.URL = url;
        this.Name = name;
        Source = source;
    }
}