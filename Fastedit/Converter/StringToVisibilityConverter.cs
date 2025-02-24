using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace Fastedit.Converter
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            if (value == null)
                return Visibility.Collapsed;

            return (value as string).Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
