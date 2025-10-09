using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Flow.Bar.Enums;

namespace Flow.Bar.Converters;

[ValueConversion(typeof(AppBarDockMode), typeof(string))]
public class DockModeToStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppBarDockMode mode && parameter is DockModeToStringType type)
        {
            var translationKey = type switch
            {
                DockModeToStringType.FollowSystemTaskbarWidthOrHeight =>
                    mode is AppBarDockMode.Left or AppBarDockMode.Right
                        ? nameof(Localize.SettingPaneAppBarSetting_FollowSystemTaskbarWidth)
                        : nameof(Localize.SettingPaneAppBarSetting_FollowSystemTaskbarHeight),
                DockModeToStringType.DockedWidthOrHeight =>
                    mode is AppBarDockMode.Left or AppBarDockMode.Right
                        ? nameof(Localize.SettingPaneAppBarSetting_DockedWidth)
                        : nameof(Localize.SettingPaneAppBarSetting_DockedHeight),
                DockModeToStringType.LeftOrTopBarElements =>
                    mode is AppBarDockMode.Left or AppBarDockMode.Right
                        ? nameof(Localize.SettingPaneAppBarSetting_TopBarElements)
                        : nameof(Localize.SettingPaneAppBarSetting_LeftBarElements),
                DockModeToStringType.RightOrBottomBarElements =>
                    mode is AppBarDockMode.Left or AppBarDockMode.Right
                        ? nameof(Localize.SettingPaneAppBarSetting_BottomBarElements)
                        : nameof(Localize.SettingPaneAppBarSetting_RightBarElements),
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(translationKey))
            {
                return App.API.GetTranslation(translationKey);
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
