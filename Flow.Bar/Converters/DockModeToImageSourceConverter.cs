using Flow.Bar.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Flow.Bar.Converters;

public class DockModeToImageSourceConverter : IValueConverter
{
    private static readonly Dictionary<AppBarDockMode, ImageSource> _cache = [];
    private static bool _isInitialized = false;

    public static async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _cache[AppBarDockMode.Top] = await App.API.LoadImageAsync(Constants.TopAppBarIcon, true);
            _cache[AppBarDockMode.Bottom] = await App.API.LoadImageAsync(Constants.BottomAppBarIcon, true);
            _cache[AppBarDockMode.Left] = await App.API.LoadImageAsync(Constants.LeftAppBarIcon, true);
            _cache[AppBarDockMode.Right] = await App.API.LoadImageAsync(Constants.RightAppBarIcon, true);
            _isInitialized = true;
        }
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AppBarDockMode dockMode && _cache.TryGetValue(dockMode, out var cachedImage))
        {
            return cachedImage;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
