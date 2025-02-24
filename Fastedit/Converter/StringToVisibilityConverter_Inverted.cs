using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace Fastedit.Converter
{
    internal class StringToVisibilityConverter_Inverted : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            if (value == null)
                return Visibility.Visible;

            return (value as string).Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
