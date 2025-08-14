using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Flow.Bar.Controls;
using Flow.Bar.Helpers.Dispatcher;
using Flow.Bar.Helpers.Image;
using Flow.Bar.Helpers.Logging;
using Flow.Bar.Helpers.Notification;
using Flow.Bar.Helpers.Windows;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;
using Flow.Bar.Views;

namespace Flow.Bar.Models;

public class PublicAPIInstance(Settings settings) : IPublicAPI
{
    private readonly Settings _settings = settings;

    private readonly Lock _saveSettingsLock = new();

    public void RestartApp(bool admin)
    {
        // Hide all active windows
        foreach (var window in WindowTracker.GetActiveWindow<Window>())
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
        FBLogger.Verbose(className, message, methodName);
    }

    public void LogDebug(string className, string message, [CallerMemberName] string methodName = "")
    {
        FBLogger.Debug(className, message, methodName);
    }

    public void LogInfo(string className, string message, [CallerMemberName] string methodName = "")
    {
        FBLogger.Information(className, message, methodName);
    }

    public void LogWarning(string className, string message, [CallerMemberName] string methodName = "")
    {
        FBLogger.Warning(className, message, methodName);
    }

    public void LogError(string className, string message, Exception? e = null, [CallerMemberName] string methodName = "")
    {
        FBLogger.Error(className, message, e, methodName);
    }

    public void LogFatal(string className, string message, Exception? e = null, [CallerMemberName] string methodName = "")
    {
        FBLogger.Fatal(className, message, e, methodName);
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
