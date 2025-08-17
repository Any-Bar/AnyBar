using System;
using System.Globalization;
using System.Windows.Data;
using Flow.Bar.Enums;
using Flow.Bar.Helpers.Plugins;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Plugin;
using Flow.Bar.ViewModels;

namespace Flow.Bar.Converters;

public class BarElementModelToFrameworkElementConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 ||
            values[0] is not BarElementModel element ||
            values[1] is not AppBarViewModel viewModel)
        {
            return null;
        }

        var isHorizontal = viewModel.DockMode is AppBarDockMode.Top or AppBarDockMode.Bottom;
        var position = element.BarElementPosition switch
        {
            BarElementModelPosition.LeftOrTop => isHorizontal ? BarElementPosition.Left : BarElementPosition.Top,
            BarElementModelPosition.Center => isHorizontal ? BarElementPosition.HorizontalCenter : BarElementPosition.VerticalCenter,
            BarElementModelPosition.RightOrBottom => isHorizontal ? BarElementPosition.Right : BarElementPosition.Bottom,
            _ => throw new NotImplementedException()
        };
        return PluginManager.CreateBarElement(element, position, viewModel.ActualDockedWidthOrHeight);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
