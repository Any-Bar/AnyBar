using System.Collections.Generic;
using System.Windows;
using AnyBar.Enums;
using iNKORE.UI.WPF.Helpers;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;

namespace AnyBar.Helpers.Windows;

public static class WindowBackdropHelper
{
    public static void SetWindowBackdrop(WindowBackdropType windowBackdropType, Window window)
    {
        SetWindowBackdrop(windowBackdropType, [window]);
    }

    public static void SetWindowBackdrop(WindowBackdropType windowBackdropType, List<Window> windows)
    {
        var type = windowBackdropType switch
        {
            WindowBackdropType.None => BackdropType.None,
            WindowBackdropType.Mica => BackdropType.Mica,
            WindowBackdropType.Acrylic => BackdropType.Acrylic,
            _ => BackdropType.None
        };

        if (OSVersionHelper.IsWindows11OrGreater)
        {
            foreach (var window in windows)
            {
                WindowHelper.SetSystemBackdropType(window, type);
            }
        }
        else if (OSVersionHelper.IsWindows10OrGreater)
        {
            var type1 = type == BackdropType.Acrylic ? BackdropType.Acrylic : BackdropType.None;
            foreach (var window in windows)
            {
                WindowHelper.SetSystemBackdropType(window, type1);
            }
        }
        else if (OSVersionHelper.IsWindowsVistaOrGreater)
        {
            var useAreo = type != BackdropType.None;
            foreach (var window in windows)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                WindowHelper.SetUseAeroBackdrop(window, useAreo);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
