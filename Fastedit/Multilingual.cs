using System;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Fastedit
{
    /// <summary>
    /// Provides localized strings from Strings/&lt;lang&gt;/Resources.resw
    /// </summary>
    public static class Multilingual
    {
        private static ResourceLoader loader;

        static Multilingual()
        {
            Reload();
        }

        /// <summary>Recreates the resource loader when active language changes.</summary>
        public static void Reload()
        {
            try
            {
                loader = new ResourceLoader();
            }
            catch
            {
                loader = null;
            }
        }

        /// <summary>Returns the localized string for the given resource key.</summary>
        public static string L(string key)
        {
            try
            {
                var value = loader?.GetString(key);
                return string.IsNullOrEmpty(value) ? key : value;
            }
            catch
            {
                return key;
            }
        }

        /// <summary>Returns a localized format string (e.g. "Welcome to Fastedit {0}!") filled with the arguments.</summary>
        public static string L(string key, params object[] args)
        {
            try
            {
                return string.Format(L(key), args);
            }
            catch
            {
                return key;
            }
        }
    }
}
