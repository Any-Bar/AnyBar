using System;
using System.Globalization;
using System.Windows.Data;
using Flow.Bar.Enums;
using Flow.Bar.Helpers.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Plugin;

namespace Flow.Bar.Converters;

public class BarElementModelToFrameworkElementConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BarElementModel element)
        {
            var isHorizontal = element.AppBar.DockMode is AppBarDockMode.Top or AppBarDockMode.Bottom;
            var position = element.BarElementPosition switch
            {
                BarElementModelPosition.LeftOrTop => isHorizontal ? BarElementPosition.Left : BarElementPosition.Top,
                BarElementModelPosition.Center => isHorizontal ? BarElementPosition.HorizontalCenter : BarElementPosition.VerticalCenter,
                BarElementModelPosition.RightOrBottom => isHorizontal ? BarElementPosition.Right : BarElementPosition.Bottom,
                _ => throw new NotImplementedException()
            };
            return PluginManager.GetBarElement(element, position);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
