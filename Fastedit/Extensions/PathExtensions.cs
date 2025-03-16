namespace Fastedit.Extensions;

public static class PathExtensions
{
    public static bool ContainsInvalidPathChars(this string path)
    {
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

        for (int i = 0; i < path.Length; i++)
        {
            for (int j = 0; j < invalidChars.Length; j++)
            {
                if (path[i] == invalidChars[j])
                {
                    return true;
                }
            }
        }
        return false;
    }

}
