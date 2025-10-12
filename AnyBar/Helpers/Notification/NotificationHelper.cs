using System;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AnyBar.Helpers.Notification;

public class NotificationHelper
{
    private const string ClassName = nameof(NotificationHelper);

    public static void Show(string title, string subTitle, string? iconPath = null)
    {
        var icon = File.Exists(iconPath) ? iconPath : Constants.DefaultIcon;

        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title ?? string.Empty, hintMaxLines: 1)
                .AddText(subTitle ?? string.Empty)
                .AddAppLogoOverride(new Uri(icon));
            builder.Show();
        }
        catch (InvalidOperationException e)
        {
            // Temporary fix for the Windows 11 notification issue
            // Possibly from 22621.1413 or 22621.1485, judging by post time of #2024
            App.API.LogFatal(ClassName, "Failed to show notification because of Windows system", e);
        }
        catch (Exception e)
        {
            App.API.LogFatal(ClassName, "Failed to show notification", e);
        }
    }
}
