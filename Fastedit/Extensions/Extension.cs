using Fastedit.Controls.Textbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fastedit.Extensions
{
    public static class Extension
    {
        public static DependencyObject FindElementByName(this DependencyObject parent, string name)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement && ((FrameworkElement)child).Name == name)
                    return child;

                var findres = FindElementByName(child, name);
                if (findres != null)
                    return findres;
            }
            return null;
        }
    }
}
