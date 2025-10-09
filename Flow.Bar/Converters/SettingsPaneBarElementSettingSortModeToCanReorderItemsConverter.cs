using System;
using System.Globalization;
using System.Windows.Data;
using Flow.Bar.Enums;

namespace Flow.Bar.Converters;

[ValueConversion(typeof(SettingsPaneBarElementSettingSortMode), typeof(bool))]
internal class SettingsPaneBarElementSettingSortModeToCanReorderItemsConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SettingsPaneBarElementSettingSortMode sortMode)
        {
            return sortMode == SettingsPaneBarElementSettingSortMode.LeftTopToRightBottom || sortMode == SettingsPaneBarElementSettingSortMode.RightBottomToLeftTop;
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
