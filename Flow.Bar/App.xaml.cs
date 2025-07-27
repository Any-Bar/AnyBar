using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Helper;
using Flow.Bar.Helper.Application;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.Storage;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;
using Flow.Bar.ViewModels;
using Flow.Bar.Views;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Flow.Bar;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
    public static IPublicAPI API { get; private set; } = null!;
    public static bool LoadingOrExiting => _mainWindow == null;

    private static readonly string ClassName = nameof(App);

    private static bool _disposed;
    private static Settings _settings = null!;
    private static SettingWindow? _mainWindow;

    private System.Windows.Forms.NotifyIcon _notifyIcon = null!;
    private readonly ContextMenu _contextMenu = new();

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    #region Main

    [STAThread]
    public static void Main()
    {
        // Initialize settings so that we can get language code
        try
        {
            var storage = new FlowBarJsonStorage<Settings>();
            _settings = storage.Load();
            _settings.SetStorage(storage);
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot load setting storage, please check local data directory", e);
            return;
        }

        // Initialize system language before changing culture info
        Internationalization.InitSystemLanguageCode();

        // Change culture info before application creation to localize WinForm windows
        if (_settings.Language != Constants.SystemLanguageCode)
        {
            Internationalization.ChangeCultureInfo(_settings.Language);
        }

        // Start the application as a single instance
        if (SingleInstance<App>.InitializeAsFirstInstance())
        {
            using var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }

    #endregion

    #region Constructor

    public App()
    {
        try
        {
            var host = Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(services => services
                .AddSingleton(_ => _settings)
                .AddSingleton<IPublicAPI, PublicAPIInstance>()
                .AddSingleton<Internationalization>()
                .AddTransient<AppBarViewModel>()
            ).Build();
            Ioc.Default.ConfigureServices(host.Services);
        }
        catch (Exception e)
        {
            ShowErrorMsgBoxAndFailFast("Cannot configure dependency injection container, please open new issue in Flow.Bar", e);
            return;
        }

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
        System.Windows.MessageBox.Show(e.ToString(), message, MessageBoxButton.OK, MessageBoxImage.Error);

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

            API.LogInfo(ClassName, "Begin Flow Bar startup ----------------------------------------------------");
            API.LogInfo(ClassName, $"Runtime info:{ErrorReporting.RuntimeInfo()}");

            RegisterAppDomainExceptions();
            RegisterDispatcherUnhandledException();

            PluginManager.LoadPlugins();

            await PluginManager.InitializePluginsAsync();

            var allPlugins = PluginManager.AllPlugins;

            _mainWindow ??= new SettingWindow();
            _mainWindow.Show();

            Current.MainWindow = _mainWindow;
            Current.MainWindow.Title = Constants.FlowBarFullName;

            InitNotifyIcon();

            var appBarKeys = _settings.AppBars.Keys.OrderBy(k => k);
            foreach (var key in appBarKeys)
            {
                var appBarModel = _settings.AppBars[key];
                var appBarViewModel = Ioc.Default.GetRequiredService<AppBarViewModel>();
                appBarViewModel.ID = key;
                appBarViewModel.DockMode = appBarModel.DockMode;
                if (appBarModel.MonitorName != null)
                {
                    appBarViewModel.Monitor = MonitorInfo.GetDisplayMonitors()
                        .FirstOrDefault(m => m.Name == appBarModel.MonitorName);
                }
                else
                {
                    appBarViewModel.Monitor = null;
                }
                appBarViewModel.DockedWidthOrHeight = appBarModel.DockedWidthOrHeight;
                appBarViewModel.IsResizable = appBarModel.IsResizable;
                var barWindow = new AppBarWindow(appBarViewModel);
                barWindow.Show();
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            RegisterExitEvents();

            API.SaveAppAllSettings();
            API.LogInfo(ClassName, "End Flow Bar startup ------------------------------------------------------");
        });
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
    /// Let exception throw as normal is better for Debug
    /// </summary>
    [Conditional("RELEASE")]
    private void RegisterDispatcherUnhandledException()
    {
        DispatcherUnhandledException += ErrorReporting.DispatcherUnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug
    /// </summary>
    [Conditional("RELEASE")]
    private static void RegisterAppDomainExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += ErrorReporting.UnhandledException;
    }

    #endregion

    #region NotifyIcon

    private void InitNotifyIcon()
    {
        var exitItem = new MenuItem
        {
            Header = API.GetTranslation("TrayIcon.Exit"),
            Icon = new FontIcon { Glyph = "\ue7e8" }
        };
        exitItem.Click += (o, e) =>
        {
            _contextMenu.IsOpen = false;
            Current.Shutdown();
        };
        _contextMenu.Items.Add(exitItem);
        _notifyIcon = new System.Windows.Forms.NotifyIcon
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
                case System.Windows.Forms.MouseButtons.Right:

                    _contextMenu.IsOpen = true;
                    // Get context menu handle and bring it to the foreground at the topmost
                    if (PresentationSource.FromVisual(_contextMenu) is HwndSource hwndSource)
                    {
                        Win32Helper.SetForegroundWindow(hwndSource.Handle);
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

        if (disposing)
        {
            // Dispose needs to be called on the main Windows thread,
            // since some resources owned by the thread need to be disposed.
        }
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

    }

    #endregion
}
