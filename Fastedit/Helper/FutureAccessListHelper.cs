using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Fastedit.Helper
{
    internal class FutureAccessListHelper
    {
        public static async Task<(bool success, StorageFile file)> GetFileAsync(string token)
        {
            if (token == null || token.Length == 0)
                return (false, null);

            try
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                if(file == null)
                    return (false, null);
                return (true, file);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
