using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;

namespace Flow.Bar.Helper.Notification;

public class NotificationHelper
{
    private const string ClassName = nameof(NotificationHelper);

    public static void Show(string title, string subTitle, string? iconPath = null)
    {
        var icon = File.Exists(iconPath) ? iconPath : null;

        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title, hintMaxLines: 1);
            if (!string.IsNullOrEmpty(subTitle))
            {
                builder.AddText(subTitle);
            }
            if (!string.IsNullOrEmpty(icon))
            {
                builder.AddAppLogoOverride(new Uri(icon));
            }
            builder.Show();
        }
        catch (Exception e)
        {
            App.API.LogFatal(ClassName, "Failed to show notification", e);
        }
    }
}
