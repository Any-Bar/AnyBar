using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Flow.Bar.Helper.Image;
using Flow.Bar.Helper.Logging;
using Flow.Bar.Helper.Notification;
using Flow.Bar.Helper.Windows;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;
using Flow.Bar.Views;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

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
        Application.Current.Dispatcher.Invoke(() =>
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

    public MessageBoxResult ShowMsgBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK)
    {
        return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
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
