using Fastedit.Extensibility;
using Fastedit.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fastedit.Helper;

internal static class ExtensionHelper
{
    public static List<Extension> Extensions = new List<Extension>();

    public static void Init()
    {
        Task.Run(new Action(async () =>
        {
            if (Directory.Exists(ApplicationData.Current.LocalFolder.Path + "\\extensions\\"))
            {
                if (File.Exists(ApplicationData.Current.LocalFolder.Path + "\\delete_extensions.dat"))
                {
                    foreach (string extensionID in File.ReadLines(ApplicationData.Current.LocalFolder.Path + "\\delete_extensions.dat"))
                    {
                        if (File.Exists(ApplicationData.Current.LocalFolder.Path + "\\extensions\\" + extensionID + ".dll"))
                        {
                            await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync("extensions")).GetFileAsync(extensionID + ".dll")).DeleteAsync();
                        }
                    }
                }

                File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "\\delete_extensions.dat", "");
                string[] extensionPaths = Directory.GetFiles(ApplicationData.Current.LocalFolder.Path + "\\extensions\\");
                Extensions.Clear();
                for (int i = 0; i < extensionPaths.Length; i++)
                {
                    if (Path.GetExtension(extensionPaths[i]).ToLower() == ".dll")
                    {
                        Extensions.Add(new Extension(ReflectionHelper.GetAllExternalInstances(extensionPaths[i]), Path.GetFileNameWithoutExtension(extensionPaths[i])));
                    }
                }
            }
        }));
    }

    public static T[] GetAllClassesWithInterface<T>() where T : IExtensionInterface
    {
        List<T> result = new List<T>();
        for (int i = 0; i < Extensions.Count; i++)
        {
            int length = Extensions[i].Interfaces.Length;
            for (int j = 0; j < length; j++)
            {
                if (Extensions[i].Interfaces[j] is T t)
                {
                    result.Add(t);
                }
            }
        }
        return result.ToArray();
    }

    public static List<FetchedExtension> GetExtensionsFromSources()
    {
        List<FetchedExtension> res = new List<FetchedExtension>();
        for (int i = 0; i < Extensions.Count; i++)
        {
            int length = Extensions[i].Interfaces.Length;
            for (int j = 0; j < length; j++)
            {
                if (Extensions[i].Interfaces[j] is IExtensionSource source)
                {
                    res.AddRange(source.GetExtensionSources().Select((item) => { return new FetchedExtension(item.Source, item.Name, source.SourceName); }));
                }
            }
        }
        return res;
    }
}