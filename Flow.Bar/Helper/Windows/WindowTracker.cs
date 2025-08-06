using System.Collections.Generic;
using System.Windows;

namespace Flow.Bar.Helper.Windows;

public static class WindowTracker
{
    public static List<Window> ActiveWindows { get; } = [];

    public static void TrackWindow(Window window)
    {
        window.Closed += (sender, args) =>
        {
            ActiveWindows.Remove(window);
        };
        ActiveWindows.Add(window);
    }
}
