using System.Collections.Generic;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Models.UserSettings;

namespace Flow.Bar.Helpers.Windows;

public static class WindowTracker
{
    private static readonly Settings _settings = Ioc.Default.GetRequiredService<Settings>();

    private static readonly List<Window> _activeWindows = [];

    public static void TrackWindow(Window window, bool setBackdrop = true)
    {
        window.Closed += (sender, args) =>
        {
            _activeWindows.Remove(window);
        };
        _activeWindows.Add(window);
        if (setBackdrop)
        {
            WindowBackdropHelper.SetWindowBackdrop(_settings.WindowBackdropType, window);
        }
    }

    public static List<Window> GetActiveWindow()
    {
        return [.. _activeWindows];
    }

    public static List<T> GetActiveWindow<T>() where T : Window
    {
        var result = new List<T>();
        foreach (var window in _activeWindows)
        {
            if (window is T typedWindow)
            {
                result.Add(typedWindow);
            }
        }
        return result;
    }
}
