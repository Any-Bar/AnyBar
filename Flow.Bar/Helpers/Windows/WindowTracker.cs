using System.Collections.Generic;
using System.Windows;

namespace Flow.Bar.Helper.Windows;

public static class WindowTracker
{
    private static readonly List<Window> _activeWindows = [];

    public static void TrackWindow(Window window)
    {
        window.Closed += (sender, args) =>
        {
            _activeWindows.Remove(window);
        };
        _activeWindows.Add(window);
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
