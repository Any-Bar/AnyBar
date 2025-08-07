using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Plugin;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Flow.Bar.Converters;

public class BarElementModelConverterToFrameworkElement : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BarElementModel element)
        {
            var isHorizontal = element.AppBar.DockMode is AppBarDockMode.Top or AppBarDockMode.Bottom;
            var position = element.BarElementPosition switch
            {
                BarElementModel.Position.LeftOrTop => isHorizontal ? BarElementPosition.Left : BarElementPosition.Top,
                BarElementModel.Position.Center => isHorizontal ? BarElementPosition.HorizontalCenter : BarElementPosition.VerticalCenter,
                BarElementModel.Position.RightOrBottom => isHorizontal ? BarElementPosition.Right : BarElementPosition.Bottom,
                _ => throw new NotSupportedException($"Unsupported BarElementPosition: {element.BarElementPosition}")
            };
            return PluginManager.GetBarElement(element, position);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
