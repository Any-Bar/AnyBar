using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Flow.Bar.Helper.Monitor;

public class MonitorInfoHelper
{
    public const int DefaultDockedWidthOrHeight = 48;

    // This dictionary is used to cache the taskbar width or height for each monitor.
    // Because we will register AppBarWindow which can cause MonitorInfo.WorkingArea changed.
    private static readonly ConcurrentDictionary<string, int> MonitorTaskBarWidthOrHeights = [];

    public static MonitorInfo? GetMonitorInfoFromName(string? monitorName)
    {
        if (monitorName == null)
        {
            var primaryMonitor = MonitorInfo.GetPrimaryDisplayMonitor();
            if (primaryMonitor != null)
            {
                return primaryMonitor;
            }
            else
            {
                var allMonitors = MonitorInfo.GetDisplayMonitors();
                return allMonitors.Length > 0 ? allMonitors[0] : null;
            }
        }
        else
        {
            var allMonitors = MonitorInfo.GetDisplayMonitors();
            return allMonitors.FirstOrDefault(m => m.Name == monitorName)
                ?? allMonitors.FirstOrDefault(m => m.IsPrimary)
                ?? (allMonitors.Length > 0 ? allMonitors[0] : null);
        }
    }

    public static int GetMonitorTaskBarWidthOrHeight(MonitorInfo? monitor)
    {
        if (monitor != null)
        {
            if (MonitorTaskBarWidthOrHeights.TryGetValue(monitor.Name, out var widthOrHeight))
            {
                return widthOrHeight;
            }

            var taskBarWidthOrHeight = (int)monitor.Bounds.Height - (int)monitor.WorkingArea.Height;
            // Taskbar is docked at the top or bottom
            if (taskBarWidthOrHeight == 0)
            {
                // Taskbar is docked at the left or right
                taskBarWidthOrHeight = (int)monitor.Bounds.Width - (int)monitor.WorkingArea.Width;
                if (taskBarWidthOrHeight > 0)
                {
                    MonitorTaskBarWidthOrHeights[monitor.Name] = taskBarWidthOrHeight;
                    return taskBarWidthOrHeight;
                }
            }
        }

        // No taskbar detected, return a default value
        return DefaultDockedWidthOrHeight;
    }

    public static (int Min, int Max, int Value) GetMinAndMaxDockedWidthOrHeight(int dockedWidthOrHeight, AppBarDockMode dockMode, MonitorInfo monitor)
    {
        int minValue, maxValue, value;
        switch (dockMode)
        {
            case AppBarDockMode.Left:
            case AppBarDockMode.Right:
                minValue = 0;
                maxValue = (int)monitor.WorkingArea.Width;
                break;
            case AppBarDockMode.Top:
            case AppBarDockMode.Bottom:
                minValue = 0;
                maxValue = (int)monitor.WorkingArea.Height;
                break;
            default:
                throw new NotImplementedException();
        }
        if (dockedWidthOrHeight < minValue)
        {
            value = maxValue;
        }
        else if (dockedWidthOrHeight > maxValue)
        {
            value = maxValue;
        }
        else
        {
            value = dockedWidthOrHeight;
        }
        return (minValue, maxValue, value);
    }
}
