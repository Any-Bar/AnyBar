using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Helper.Application;
using Flow.Bar.Models;
using Flow.Bar.Models.Storage;
using Flow.Bar.Models.UserSettings;
using Flow.Bar.Plugin;
using Flow.Bar.ViewModels;
using Flow.Bar.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace Flow.Bar;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
    public static IPublicAPI API { get; private set; } = null!;
    public static bool LoadingOrExiting => _mainWindow == null;

    private static bool _disposed;
    private static Settings _settings = null!;
    private static SettingWindow? _mainWindow;

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    #region Main

    [STAThread]
    public static void Main()
    {
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
        MessageBox.Show(e.ToString(), message, MessageBoxButton.OK, MessageBoxImage.Error);

        // Flow cannot construct its App instance, so ensure Flow crashes w/ the exception info.
        Environment.FailFast(message, e);
    }

    #endregion

    #region App Events

    private void OnStartup(object sender, StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        RegisterAppDomainExceptions();
        RegisterDispatcherUnhandledException();

        _mainWindow ??= new SettingWindow();
        _mainWindow.Show();

        Current.MainWindow = _mainWindow;
        Current.MainWindow.Title = Constants.FlowBarFullName;

        var barWindow = new AppBarWindow(Ioc.Default.GetRequiredService<AppBarViewModel>());
        barWindow.Show();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        RegisterExitEvents();

        API.SaveAppAllSettings();
    }

    #endregion

    #region Register Events

    private void RegisterExitEvents()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            //API.LogInfo(ClassName, "Process Exit");
            Dispose();
        };

        Current.Exit += (s, e) =>
        {
            //API.LogInfo(ClassName, "Application Exit");
            Dispose();
        };

        Current.SessionEnding += (s, e) =>
        {
            //API.LogInfo(ClassName, "Session Ending");
            Dispose();
        };
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug
    /// </summary>
    [Conditional("RELEASE")]
    private void RegisterDispatcherUnhandledException()
    {
        //DispatcherUnhandledException += ErrorReporting.DispatcherUnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug
    /// </summary>
    [Conditional("RELEASE")]
    private static void RegisterAppDomainExceptions()
    {
        //AppDomain.CurrentDomain.UnhandledException += ErrorReporting.UnhandledException;
    }

    /// <summary>
    /// Let exception throw as normal is better for Debug
    /// </summary>
    private static void RegisterTaskSchedulerUnhandledException()
    {
        //TaskScheduler.UnobservedTaskException += ErrorReporting.TaskSchedulerUnobservedTaskException;
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
