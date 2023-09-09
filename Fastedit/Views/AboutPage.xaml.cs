using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Fastedit.Views
{
    public sealed partial class AboutPage : Page
    {
        public string donoURL = "https://www.paypal.com/donate?business=julius@frozenassassine.de&no_recurring=0&item_name=Support+FrozenAssassines+Work&currency_code=EUR";
        public string AppVersion =>
            Package.Current.Id.Version.Major + "." +
            Package.Current.Id.Version.Minor + "." +
            Package.Current.Id.Version.Build;

        public string DeveloperName => Package.Current.PublisherDisplayName;

        public AboutPage()
        {
            this.InitializeComponent();

            SetChangelog();
        }

        private void SetChangelog()
        {
            //Simple parser to make headlines bigger and add paragraphs
            var data = File.ReadAllLines(@"Assets\changelog.txt");
            List<Paragraph> paragraphs = new List<Paragraph> { new Paragraph() };
            for (int i = 0; i < data.Length; i++)
            {
                string currentLine = data[i];
                Run line = new Run();
                
                //Headline:
                if (currentLine.StartsWith("#"))
                {
                    currentLine = currentLine.Remove(0, 1);
                    line.FontSize = 24;
                }
                //Paragraph
                else if (currentLine.StartsWith("---"))
                {
                    currentLine = currentLine.Remove(0, 3);
                    paragraphs.Add(new Paragraph());
                }

                line.Text = currentLine + "\n";
                paragraphs[paragraphs.Count - 1].Inlines.Add(line);
            }
            foreach (var paragraph in paragraphs)
            {
                ChangelogDisplay.Blocks.Add(paragraph);
            }
        }

        private async void NavigateToLink_Click(Controls.SettingsControl sender)
        {
            if (sender.Tag == null)
                return;

            await Windows.System.Launcher.LaunchUriAsync(new Uri(sender.Tag.ToString()));
        }
    }
}
