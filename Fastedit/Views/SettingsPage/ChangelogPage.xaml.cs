using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fastedit.Views.SettingsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangelogPage : Page
    {
        public ChangelogPage()
        {
            this.InitializeComponent();
            ReadFromFile();
        }

        private async void ReadFromFile()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/news.txt"));
            if (file == null) return;

            NewsDisplayTextblock.Text = await FileIO.ReadTextAsync(file);
        }
    }
}
