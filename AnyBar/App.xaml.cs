using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using AnyBar.Helpers.Application;
using AnyBar.Helpers.Image;
using AnyBar.Helpers.Logging;
using AnyBar.Helpers.Plugins;
using AnyBar.Helpers.Startup;
using AnyBar.Models.Language;
using AnyBar.Models.PublicAPI;
using AnyBar.Models.Storage;
using AnyBar.Models.Taskbar;
using AnyBar.Models.UserSettings;
using AnyBar.Plugin;
using AnyBar.Services;
using AnyBar.ViewModels;
using AnyBar.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Win32;
using MessageBox = System.Windows.MessageBox;

namespace AnyBar;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
    public static IPublicAPI API { get; private set; } = null!;
    public static Settings Settings { get; private set; } = new();

    private static readonly string ClassName = nameof(App);

    private SystemTrayIcon _notifyIcon = null!;
    private readonly ContextMenu _contextMenu = new();

    private static bool _disposed;

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    #region Main

    [STAThread]
    public static void Main()
    {
        // Initialize settings so that we can get language code
        try
        {
            var storage = new AnyBarJsonStorage<Settings>();
            Settings = storage.Load();
            Settings.SetStorage(storage);
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot load setting storage, please check local data directory", e);
            return;
        }

        // Initialize system language before changing culture info
        Internationalization.InitSystemLanguageCode();

        // Change culture info before application creation to localize WinForm windows
        if (Settings.Language != Constants.SystemLanguageCode)
        {
            Internationalization.ChangeCultureInfo(Settings.Language);
        }

        // Start the application as a single instance
        if (SingleInstance<App>.InitializeAsFirstInstance())
        {
            // Set up Logging
            AnyBarLogger.Initialize();

            using var application = new App();
            application.InitializeComponent();
            application.Run();

            // Close and flush the logger
            AnyBarLogger.Close();
        }
    }

    #endregion

    #region Constructor

    public App()
    {
        // Check if the application is running as administrator
        if (Settings.AlwaysRunAsAdministrator && !PInvokeHelper.IsAdministrator())
        {
            RestartApp(true);
            return;
        }

        // Do not use bitmap cache since it can cause WPF second window freezing issue
        ShadowAssist.UseBitmapCache = false;

        // Configure the dependency injection container
        try
        {
            var host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureFBLogger()
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateOnBuild = true;
                })
                .ConfigureServices(services => services
                    .AddSingleton(_ => Settings)
                    .AddSingleton<IPublicAPI, PublicAPIInstance>()
                    .AddSingleton<Internationalization>()
                    .AddSingleton<NavigationViewService>()
                    .AddSingleton<PageService>()
                    .AddSingleton<AppBarManagementService>()
                    .AddTransient<SettingViewModel>()
                    .AddTransient<AppBarViewModel>()
                    .AddTransient<SettingsPaneAboutViewModel>()
                    .AddTransient<SettingsPaneAppBarViewModel>()
                    .AddTransient<SettingsPaneGeneralViewModel>()
                    .AddTransient<SettingsPaneAppBarSettingViewModel>()
                    .AddTransient<SettingsPanePluginsViewModel>()
                    .AddTransient<SettingsPaneBarElementSettingViewModel>()
            ).Build();
            Ioc.Default.ConfigureServices(host.Services);
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot configure dependency injection container, please open new issue in AnyBar", e);
            return;
        }

        // Initialize the public API first
        try
        {
            API = Ioc.Default.GetRequiredService<IPublicAPI>();
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot initialize api, please open new issue in AnyBar", e);
            return;
        }
    }

    #endregion

    #region Fail Fast

    private static void ShowErrorMsgBoxAndFailFast(string message, Exception e)
    {
        // Firstly show users the message
        MessageBox.Show(e.ToString(), message, MessageBoxButton.OK, MessageBoxImage.Error);

        // App cannot construct its App instance, so ensure app crashes w/ the exception info.
        Environment.FailFast(message, e);
    }

    #endregion

    #region App Events

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await API.StopwatchLogInfoAsync(ClassName, "Startup cost", async () =>
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize language before portable clean up since it needs translations
            await Ioc.Default.GetRequiredService<Internationalization>().InitializeLanguageAsync();

            API.LogInfo(ClassName, "Begin AnyBar startup -------------------------------------------------");
            API.LogInfo(ClassName, $"Runtime info:{ExceptionFormatter.RuntimeInfo()}");

            RegisterAppDomainExceptions();
            RegisterDispatcherUnhandledException();
            RegisterTaskSchedulerUnhandledException();

            var imageLoadertask = ImageLoader.InitializeAsync();

            PluginManager.LoadPlugins();

            await PluginManager.InitializePluginsAsync();

            // Update plugin titles after plugins are initialized with their api instances
            Internationalization.UpdatePluginMetadataTranslations();

            await imageLoadertask;

            InitNotifyIcon();

            if (!Settings.HideSettingWindow)
            {
                var settingWindow = new SettingWindow();
                settingWindow.Show();
            }

            Ioc.Default.GetRequiredService<AppBarManagementService>().InitializeAllAppBarWindows();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            RegisterExitEvents();

            AutoStartup();

            API.SaveAppAllSettings();
            API.LogInfo(ClassName, "End AnyBar startup ---------------------------------------------------");
        });
    }

    /// <summary>
    /// Check startup only for Release.
    /// </summary>
    [Conditional("RELEASE")]
    private static void AutoStartup()
    {
        // We try to enable auto-startup on first launch, or reenable if it was removed
        if (Settings.StartOnSystemStartup)
        {
            try
            {
                AutoStartupHelper.CheckIsEnabled(Settings.UseLogonTaskForStartup, Settings.AlwaysRunAsAdministrator);
            }
            catch (UnauthorizedAccessException)
            {
                // If it fails for permission, we need to ask the user to restart as administrator
                if (API.ShowMsgBox(
                    Localize.SettingPaneGeneral_RestartToApplyChangeDescription(),
                    Localize.SettingPaneGeneral_RestartToApplyChange(),
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RestartApp(true);
                }
            }
            catch (Exception)
            {
                // If it fails (permissions, etc) then don't keep retrying,
                // just disable auto-startup to give the user a visual indication in the settings window
                Settings.StartOnSystemStartup = false;
                API.ShowMsgError(Localize.App_FailedToSetAutoStartup());
                // No need to log error here since it is already logged in AutoStartupHelper
            }
        }
    }

    #endregion

    #region Register Events

    private void RegisterExitEvents()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            API.LogInfo(ClassName, "Process Exit");
            Dispose();
        };

        Current.Exit += (s, e) =>
        {
            API.LogInfo(ClassName, "Application Exit");
            Dispose();
        };

        Current.SessionEnding += (s, e) =>
        {
            API.LogInfo(ClassName, "Session Ending");
            Dispose();
        };
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug.
    /// </summary>
    [Conditional("RELEASE")]
    private void RegisterDispatcherUnhandledException()
    {
        DispatcherUnhandledException += ErrorReporting.DispatcherUnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug.
    /// </summary>
    [Conditional("RELEASE")]
    private static void RegisterAppDomainExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += ErrorReporting.UnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug.
    /// </summary>
    private static void RegisterTaskSchedulerUnhandledException()
    {
        TaskScheduler.UnobservedTaskException += ErrorReporting.TaskSchedulerUnobservedTaskException;
    }

    #endregion

    #region Restart

    public static void RestartApp(bool admin, string? param = null)
    {
        if (!string.IsNullOrEmpty(Constants.ExecutablePath) && File.Exists(Constants.ExecutablePath))
        {
            // Start a new instance of the application
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Constants.ExecutablePath,
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    Arguments = param,
                    Verb = admin ? "runas" : string.Empty
                });
            }
            catch (Exception)
            {
                // Ignore any exceptions that occur while starting the new process
            }

            // Close the log
            AnyBarLogger.Close();

            // Kill the current process
            Process.GetCurrentProcess().Kill();
        }
    }

    #endregion

    #region NotifyIcon

    private void InitNotifyIcon()
    {
        var exitItem = new MenuItem
        {
            Header = Localize.TrayIcon_Exit(),
            Icon = new FontIcon { Glyph = "\ue7e8" }
        };
        exitItem.Click += (o, e) =>
        {
            _contextMenu.IsOpen = false;
            Current.Shutdown();
        };
        var settingItem = new MenuItem
        {
            Header = Localize.SettingWindow_Title(),
            Icon = new FontIcon { Glyph = "\ue713" }
        };
        settingItem.Click += (o, e) =>
        {
            API.ShowSettingWindow();
        };
        _contextMenu.Items.Add(settingItem);
        _contextMenu.Items.Add(exitItem);
        _notifyIcon = new SystemTrayIcon
        {
            Tooltip = Constants.AnyBarFullName,
            Icon = new(Constants.AppIcon)
        };
        _notifyIcon.RightClick += (o, e) =>
        {
            _contextMenu.IsOpen = true;
            // Get context menu handle and bring it to the foreground at the topmost
            if (PresentationSource.FromVisual(_contextMenu) is HwndSource hwndSource)
            {
                PInvokeHelper.SetForegroundWindow(hwndSource.Handle);
            }
            _contextMenu.Focus();
        };
        _notifyIcon.Show();
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        // Prevent two disposes at the same time.
        lock (_disposingLock)
        {
            if (!disposing)
            {
                return;
            }

            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        API.StopwatchLogInfo(ClassName, "Dispose cost", () =>
        {
            API.LogInfo(ClassName, "Begin AnyBar dispose -------------------------------------------------");

            if (disposing)
            {
                // Dispose needs to be called on the main Windows thread,
                // since some resources owned by the thread need to be disposed.
                _notifyIcon.Hide();
                _notifyIcon.Dispose();
                Ioc.Default.GetRequiredService<AppBarManagementService>().Dispose();
                ToastNotificationManagerCompat.Uninstall();
                API.SaveAppAllSettings();
            }

            API.LogInfo(ClassName, "End AnyBar dispose ---------------------------------------------------");
        });
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ISingleInstanceApp

    public void OnSecondAppStarted()
    {
        API.ShowSettingWindow();
    }

    #endregion
}
