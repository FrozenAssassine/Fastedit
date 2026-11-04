using Fastedit.Core;
using System;

namespace Fastedit
{
    /// <summary>
    /// Provides localized strings from Strings/<lang>/Resources.resw
    /// </summary>
    public static class Multilingual
    {
        /// <summary>Recreates or re-syncs the resource loader when active language changes.</summary>
        public static void Reload()
        {
            // Context and qualifier switching is maintained through LocalizationManager
        }

        /// <summary>Returns the localized string for the given resource key.</summary>
        public static string L(string key)
        {
            try
            {
                return LocalizationManager.Instance.GetString(key).Replace("\\n", "\n");
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
