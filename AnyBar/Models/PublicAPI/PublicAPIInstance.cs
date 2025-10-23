using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AnyBar.Controls;
using AnyBar.Helpers.Dispatcher;
using AnyBar.Helpers.Image;
using AnyBar.Helpers.Logging;
using AnyBar.Helpers.Notification;
using AnyBar.Helpers.Windows;
using AnyBar.Models.Language;
using AnyBar.Models.UserSettings;
using AnyBar.Plugin;
using AnyBar.Views;

namespace AnyBar.Models.PublicAPI;

public class PublicAPIInstance(Settings settings) : IPublicAPI
{
    private readonly Settings _settings = settings;

    private readonly Lock _saveSettingsLock = new();

    public void RestartApp(bool admin)
    {
        // Hide all active windows
        foreach (var window in WindowTracker.GetActiveWindows<Window>())
        {
            window.Hide();
        }

        // Save all settings before restarting
        SaveAppAllSettings();

        App.RestartApp(admin);
    }

    public void ShowSettingWindow()
    {
        DispatcherHelper.RunOnMainThread(() =>
        {
            var settingWindow = SingletonWindowOpener.Open<SettingWindow>();
        });
    }

    public void SaveAppAllSettings()
    {
        lock (_saveSettingsLock)
        {
            _settings.Save();
            ImageLoader.Save();
        }
    }

    public string GetTranslation(string key)
    {
        return Internationalization.GetTranslation(key);
    }

    public void ShowMsg(string title, string subTitle = "", string iconPath = "")
    {
        NotificationHelper.Show(title, subTitle, iconPath);
    }

    public void ShowMsgError(string title, string subTitle = "")
    {
        NotificationHelper.Show(title, subTitle, Constants.ErrorIcon);
    }

    public MessageBoxResult ShowMsgBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK)
    {
        var iconSource = icon switch
        {
            MessageBoxImage.None => null,
            MessageBoxImage.Error => ImageLoader.ErrorImage,
            MessageBoxImage.Question => ImageLoader.QuestionImage,
            MessageBoxImage.Warning => ImageLoader.WarningImage,
            MessageBoxImage.Information => ImageLoader.InformationImage,
            _ => null
        };
        return MessageBoxEx.Show(messageBoxText, caption, button, iconSource, defaultResult, null);
    }

    public void LogVerbose(string className, string message, [CallerMemberName] string methodName = "")
    {
        AnyBarLogger.Verbose(className, message, methodName);
    }

    public void LogDebug(string className, string message, [CallerMemberName] string methodName = "")
    {
        AnyBarLogger.Debug(className, message, methodName);
    }

    public void LogInfo(string className, string message, [CallerMemberName] string methodName = "")
    {
        AnyBarLogger.Information(className, message, methodName);
    }

    public void LogWarning(string className, string message, [CallerMemberName] string methodName = "")
    {
        AnyBarLogger.Warning(className, message, methodName);
    }

    public void LogError(string className, string message, Exception? e = null, [CallerMemberName] string methodName = "")
    {
        AnyBarLogger.Error(className, message, e, methodName);
    }

    public void LogFatal(string className, string message, Exception? e = null, [CallerMemberName] string methodName = "")
    {
        AnyBarLogger.Fatal(className, message, e, methodName);
    }

    public long StopwatchLogDebug(string className, string message, Action action, [CallerMemberName] string methodName = "")
    {
        return Stopwatch.Debug(className, message, action, methodName);
    }

    public Task<long> StopwatchLogDebugAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "")
    {
        return Stopwatch.DebugAsync(className, message, action, methodName);
    }

    public long StopwatchLogInfo(string className, string message, Action action, [CallerMemberName] string methodName = "")
    {
        return Stopwatch.Info(className, message, action, methodName);
    }

    public Task<long> StopwatchLogInfoAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "")
    {
        return Stopwatch.InfoAsync(className, message, action, methodName);
    }

    public ValueTask<ImageSource?> LoadImageAsync(string path, bool loadFullImage = false, bool cacheImage = true)
    {
        return ImageLoader.LoadAsync(path, loadFullImage, cacheImage);
    }
}
