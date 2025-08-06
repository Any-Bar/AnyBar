using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Flow.Bar.Plugin;

/// <summary>
/// Public APIs that plugin can use
/// </summary>
public interface IPublicAPI
{
    /// <summary>
    /// Open setting dialog
    /// </summary>
    void OpenSettingDialog();

    /// <summary>
    /// Save everything, all of Flow Bar and plugins' data and settings
    /// </summary>
    void SaveAppAllSettings();

    /// <summary>
    /// Get translation of current language
    /// You need to implement IPluginI18n if you want to support multiple languages for your plugin
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string GetTranslation(string key);

    /// <summary>
    /// Show message box
    /// </summary>
    /// <param name="title">Message title</param>
    /// <param name="subTitle">Message subtitle</param>
    /// <param name="iconPath">Message icon path (relative path to your plugin folder)</param>
    void ShowMsg(string title, string subTitle = "", string iconPath = "");

    /// <summary>
    /// Displays a standardised Flow message box.
    /// </summary>
    /// <param name="messageBoxText">The message of the message box.</param>
    /// <param name="caption">The caption of the message box.</param>
    /// <param name="button">Specifies which button or buttons to display.</param>
    /// <param name="icon">Specifies the icon to display.</param>
    /// <param name="defaultResult">Specifies the default result of the message box.</param>
    /// <returns>Specifies which message box button is clicked by the user.</returns>
    public MessageBoxResult ShowMsgBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK);

    /// <summary>
    /// Log debug message
    /// Message will only be logged in Debug mode
    /// </summary>
    void LogDebug(string className, string message, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log info message
    /// </summary>
    void LogInfo(string className, string message, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log warning message
    /// </summary>
    void LogWarning(string className, string message, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log error message. Preferred error logging method for plugins.
    /// </summary>
    void LogError(string className, string message, Exception e = null, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log fatal message. Will throw if in debug mode so developer will be aware,
    /// otherwise logs the eror message. This is the primary logging method used for Flow
    /// </summary>
    void LogFatal(string className, string message, Exception e = null, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log debug message of the time taken to execute a method
    /// Message will only be logged in Debug mode
    /// </summary>
    /// <returns>The time taken to execute the method in milliseconds</returns>
    public long StopwatchLogDebug(string className, string message, Action action, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log debug message of the time taken to execute a method asynchronously
    /// Message will only be logged in Debug mode
    /// </summary>
    /// <returns>The time taken to execute the method in milliseconds</returns>
    public Task<long> StopwatchLogDebugAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log info message of the time taken to execute a method
    /// </summary>
    /// <returns>The time taken to execute the method in milliseconds</returns>
    public long StopwatchLogInfo(string className, string message, Action action, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Log info message of the time taken to execute a method asynchronously
    /// </summary>
    /// <returns>The time taken to execute the method in milliseconds</returns>
    public Task<long> StopwatchLogInfoAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "");

    /// <summary>
    /// Load image from path.
    /// Support local, remote and data:image url.
    /// Support png, jpg, jpeg, gif, bmp, tiff, ico, svg image files.
    /// If image path is missing, it will return a missing icon.
    /// </summary>
    /// <param name="path">The path of the image.</param>
    /// <param name="loadFullImage">
    /// Load full image or not.
    /// </param>
    /// <param name="cacheImage">
    /// Cache the image or not. Cached image will be stored in FL cache.
    /// If the image is just used one time, it's better to set this to false.
    /// </param>
    /// <returns></returns>
    ValueTask<ImageSource> LoadImageAsync(string path, bool loadFullImage = false, bool cacheImage = true);
}
