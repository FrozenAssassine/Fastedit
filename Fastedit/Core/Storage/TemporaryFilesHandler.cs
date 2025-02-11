using Fastedit.Core.Settings;
using Fastedit.Helper;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fastedit.Storage
{
    internal class TemporaryFilesHandler
    {
        public static async Task<bool> Clear()
        {
            try
            {
                if (!Directory.Exists(DefaultValues.TemporaryFilesPath))
                    Directory.CreateDirectory(DefaultValues.TemporaryFilesPath);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(DefaultValues.TemporaryFilesPath);
                var files = await folder.GetFilesAsync();
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i] == null)
                        return false;
                    await files[i].DeleteAsync();
                }

                //check if all have been deleted
                var files_2 = await folder.GetFilesAsync();
                return files_2.Count == 0;
            }
            catch
            {
                return false;
            }
        }

        public static string GetSize()
        {
            if (!Directory.Exists(DefaultValues.TemporaryFilesPath))
                Directory.CreateDirectory(DefaultValues.TemporaryFilesPath);

            return SizeCalculationHelper.CalculateFolderSize(DefaultValues.TemporaryFilesPath);
        }
    }
}
