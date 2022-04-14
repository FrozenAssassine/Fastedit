using Fastedit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fastedit.Helper
{
    public class VersionHelper
    {
        public static WindowsVersion GetWindowsVersion()
        {
            try
            {
                string Version = Environment.OSVersion.ToString();
                if (Version.Length == 0)
                    return WindowsVersion.Windows10;

                Version = Version.Remove(0, 26);
                if (Version.Length == 0)
                    return WindowsVersion.Windows10;

                int indexof = Version.IndexOf(".");
                if (indexof > -1)
                    Version = Version.Remove(indexof, Version.Length - indexof);
                int res = 0;
                int.TryParse(Version, out res);
                if (res >= 22000)
                    return WindowsVersion.Windows11;
                else
                    return WindowsVersion.Windows10;
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is ArgumentNullException || ex is InvalidOperationException)
            {
                return WindowsVersion.Windows10;
            }
        }
    }
}
