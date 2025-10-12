using System;
using System.Globalization;
using System.Windows.Data;
using AnyBar.Enums;

namespace AnyBar.Converters;

[ValueConversion(typeof(SettingsPaneAppBarSortMode), typeof(bool))]
internal class SettingsPaneAppBarSortModeToCanReorderItemsConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SettingsPaneAppBarSortMode sortMode)
        {
            return sortMode == SettingsPaneAppBarSortMode.Order;
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
