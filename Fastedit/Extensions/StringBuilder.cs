using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using System.Diagnostics;

namespace Fastedit.Extensions
{
    public class StringBuilder
    {
        public static string CleanUp(string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", "");
        }

        public static int ToNumber(string number, int defaultout = 0)
        {
            return Convert.ToInt(number, defaultout);
        }

        public static bool IsAllLetter(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsLetter(str, i))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool IsAllLetterOrDigit(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsLetterOrDigit(str, i))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool IsAllNumber(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsNumber(str, i))
                {
                    return false;
                }
            }

            return true;
        }

        public static string ReplaceFirstOccurenceInString(string text, string Find, string replace)
        {
            int pos = text.IndexOf(Find);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + Find.Length);
        }
        public static string ReplaceLastOccurenceInString(string txt, string Find, string Replace)
        {
            int place = txt.LastIndexOf(Find);

            if (place == -1)
            {
                return txt;
            }
            return txt.Remove(place, Find.Length).Insert(place, Replace);
        }

        public static bool IsBool(string str)
        {
            return bool.TryParse(str, out bool res);
        }

        public static string GetStringFromImportedData(string[] source, string find)
        {
            string item = String.Join("", source.Where(a => a.Contains(find + "=", StringComparison.Ordinal)));
            var splitted = item.Split('='); 
            if (splitted.Length > 0)
            {
                return splitted[1];
            }
            return "";
        }

        public static bool IsValidFilename(string str)
        {
            return !string.IsNullOrEmpty(str) &&
              str.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
              !File.Exists(str);
        }

        public static int IndexOfWholeWord(string Text, string Word, int StartIndex)
        {
            for (int j = StartIndex; j < Text.Length &&
                (j = Text.IndexOf(Word, j, StringComparison.Ordinal)) >= 0; j++)
            {
                if ((j == 0 || !char.IsLetterOrDigit(Text, j - 1)) &&
                    (j + Word.Length == Text.Length || !char.IsLetterOrDigit(Text, j + Word.Length)))
                {
                    return j;
                }
            }

            return -1;
        }
        public static int LastIndexOfWholeWord(string Text, string Word)
        {
            int StartIndex = Text.Length - 1;
            while (StartIndex >= 0 && (StartIndex = Text.LastIndexOf(Word, StartIndex, StringComparison.Ordinal)) != -1)
            {
                if (StartIndex > 0)
                {
                    if (!char.IsLetterOrDigit(Text[StartIndex - 1]))
                    {
                        return StartIndex;
                    }
                }

                if (StartIndex + Text.Length < Text.Length)
                {
                    if (!char.IsLetterOrDigit(Text[StartIndex + Word.Length]))
                    {
                        return StartIndex;
                    }
                }

                StartIndex--;
            }
            return -1;
        }
        public static string GetAbsolutePath(string path, string dir)
        {
            if (path.StartsWith("\"") && path.Length > 1)
            {
                var index = path.IndexOf('\"', 1);
                if (index == -1) 
                    return null;
                path = path.Substring(1, index - 1);
            }

            path = path.Trim('/').Replace('/', Path.DirectorySeparatorChar);

            if (!String.IsNullOrWhiteSpace(path)
                   && path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1
                   && Path.IsPathRooted(path)
                   && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return path;
            }

            if (path.StartsWith(".\\"))
                path = dir + Path.DirectorySeparatorChar + path.Substring(2, path.Length - 2);
            else if (path.StartsWith("..\\"))
                path = GetAbsolutePath(dir, path);
            else
                path = dir + Path.DirectorySeparatorChar + path;
            return path;
        }

    }
    public static class Extensions
    {
        public static int findIndex<T>(this T[] array, T item)
        {
            return Array.IndexOf(array, item);
        }
    }

}