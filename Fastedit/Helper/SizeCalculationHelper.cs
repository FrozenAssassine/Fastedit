using System.IO;
using System.Linq;

namespace Fastedit.Helper;

internal class SizeCalculationHelper
{
    public static string SplitSize(ulong size)
    {
        if (size < 1_000)
            return size + "B";
        else if (size > 1_000 && size < 1_000_000)
            return (size / 1_000) + "KB";
        else if (size > 1_000_000)
            return (size / 1_000_000) + "MB";
        return "";
    }

    public static string CalculateFolderSize(string path)
    {
        if (path == null || path.Length == 0)
            return "";

        DirectoryInfo di = new DirectoryInfo(path);
        return SplitSize((ulong)di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length));
    }
}
