using Microsoft.UI.Xaml.Controls;

namespace Fastedit.Models
{
    public class SettingsNavigationParameter
    {
        public SettingsNavigationParameter(MainPage mainPage, TabView tabView, string page = null)
        {
            MainPage = mainPage;
            TabView = tabView;
            Page = page;
        }
        public MainPage MainPage { get; set; }
        public TabView TabView { get; set; }
        public string Page { get; set; }
    }
}
