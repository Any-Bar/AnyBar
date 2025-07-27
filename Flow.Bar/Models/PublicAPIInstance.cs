using Flow.Bar.Helper.Windows;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;
using Flow.Bar.Views;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Flow.Bar.Models;

public class PublicAPIInstance(Settings settings) : IPublicAPI
{
    private readonly Settings _settings = settings;

    private readonly Lock _saveSettingsLock = new();

    public void OpenSettingDialog()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            SingletonWindowOpener.Open(() => new SettingWindow());
        });
    }

    public void SaveAppAllSettings()
    {
        lock (_saveSettingsLock)
        {
            _settings.Save();
        }
    }

    public string GetTranslation(string key) => Internationalization.GetTranslation(key);

    public MessageBoxResult ShowMsgBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK)
    {
        return iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
    }

    public void LogDebug(string className, string message, [CallerMemberName] string methodName = "")
    {

    }

    public void LogInfo(string className, string message, [CallerMemberName] string methodName = "")
    {

    }

    public void LogWarn(string className, string message, [CallerMemberName] string methodName = "")
    {

    }

    public void LogError(string className, string message, [CallerMemberName] string methodName = "")
    {

    }

    public void LogException(string className, string message, Exception e, [CallerMemberName] string methodName = "")
    {

    }

    public long StopwatchLogDebug(string className, string message, Action action, [CallerMemberName] string methodName = "")
    {
        action();
        return 0;
    }

    public async Task<long> StopwatchLogDebugAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "")
    {
        await action();
        return 0;
    }

    public long StopwatchLogInfo(string className, string message, Action action, [CallerMemberName] string methodName = "")
    {
        action();
        return 0;
    }

    public async Task<long> StopwatchLogInfoAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "")
    {
        await action();
        return 0;
    }
}
