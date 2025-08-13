using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Helper.Application;
using Flow.Bar.Helper.Image;
using Flow.Bar.Helper.Logging;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Helpers.Startup;
using Flow.Bar.Models;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.Storage;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;
using Flow.Bar.Services;
using Flow.Bar.ViewModels;
using Flow.Bar.Views;
using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Win32;
using MessageBox = System.Windows.MessageBox;
using MouseButtons = System.Windows.Forms.MouseButtons;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace Flow.Bar;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
    public static IPublicAPI API { get; private set; } = null!;
    public static Settings Settings { get; private set; } = new();

    private static readonly string ClassName = nameof(App);

    private static bool _disposed;

    private NotifyIcon _notifyIcon = null!;
    private readonly ContextMenu _contextMenu = new();

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    #region Main

    [STAThread]
    public static void Main()
    {
        // Set up Logging
        FBLogger.Initialize();

        // Initialize settings so that we can get language code
        try
        {
            var storage = new FlowBarJsonStorage<Settings>();
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
            using var application = new App();
            application.InitializeComponent();
            application.Run();
            FBLogger.Close();
        }
    }

    #endregion

    #region Constructor

    public App()
    {
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
            ShowErrorMsgBoxAndFailFast("Cannot configure dependency injection container, please open new issue in Flow.Bar", e);
            return;
        }

        // Initialize the public API first
        try
        {
            API = Ioc.Default.GetRequiredService<IPublicAPI>();
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot initialize api, please open new issue in Flow.Bar", e);
            return;
        }
    }

    #endregion

    #region Fail Fast

    private static void ShowErrorMsgBoxAndFailFast(string message, Exception e)
    {
        // Firstly show users the message
        MessageBox.Show(e.ToString(), message, MessageBoxButton.OK, MessageBoxImage.Error);

        // Flow cannot construct its App instance, so ensure Flow crashes w/ the exception info.
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

            API.LogInfo(ClassName, "Begin Flow Bar startup -------------------------------------------------");
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

            if (!Settings.HideSettingWindow)
            {
                var settingWindow = new SettingWindow();
                settingWindow.Show();
            }

            InitNotifyIcon();

            Ioc.Default.GetRequiredService<AppBarManagementService>().InitializeAllAppBarWindows();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            RegisterExitEvents();

            AutoStartup();

            API.SaveAppAllSettings();
            API.LogInfo(ClassName, "End Flow Bar startup ---------------------------------------------------");
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
                AutoStartupHelper.CheckIsEnabled(Settings.UseLogonTaskForStartup);
            }
            catch (Exception e)
            {
                // If it fails (permissions, etc) then don't keep retrying,
                // just disable auto-startup to give the user a visual indication in the settings window
                Settings.StartOnSystemStartup = false;
                API.ShowMsg(Localize.App_FailedToSetAutoStartup(), e.Message);
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
        API.LogInfo(ClassName, "Begin Flow Bar restart");

        if (!string.IsNullOrEmpty(Constants.ExecutablePath) && File.Exists(Constants.ExecutablePath))
        {
            // Start a new instance of the application
            Process.Start(new ProcessStartInfo
            {
                FileName = Constants.ExecutablePath,
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = param,
                Verb = admin ? "runas" : string.Empty
            });

            // Close the log
            FBLogger.Close();

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
        _notifyIcon = new NotifyIcon
        {
            Text = Constants.FlowBarFullName,
#if DEBUG
            Icon = Flow.Bar.Properties.Resource.dev,
#else
            Icon = Flow.Bar.Properties.Resource.app,
#endif
            Visible = true
        };
        _notifyIcon.MouseClick += (o, e) =>
        {
            switch (e.Button)
            {
                case MouseButtons.Right:

                    _contextMenu.IsOpen = true;
                    // Get context menu handle and bring it to the foreground at the topmost
                    if (PresentationSource.FromVisual(_contextMenu) is HwndSource hwndSource)
                    {
                        PInvokeHelper.SetForegroundWindow(hwndSource.Handle);
                    }
                    _contextMenu.Focus();

                    break;
            }
        };
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
            API.LogInfo(ClassName, "Begin Flow Bar dispose -------------------------------------------------");

            if (disposing)
            {
                // Dispose needs to be called on the main Windows thread,
                // since some resources owned by the thread need to be disposed.
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Ioc.Default.GetRequiredService<AppBarManagementService>().Dispose();
                ToastNotificationManagerCompat.Uninstall();
                API.SaveAppAllSettings();
            }

            API.LogInfo(ClassName, "End Flow Bar dispose ---------------------------------------------------");
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
