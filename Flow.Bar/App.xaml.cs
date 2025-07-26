using Flow.Bar.Helper;
using Flow.Bar.Views;
using System.Windows;

namespace Flow.Bar;

public partial class App : Application, IDisposable, ISingleInstanceApp
{
    public static bool LoadingOrExiting => _mainWindow == null;

    private static bool _disposed;
    private static SettingWindow? _mainWindow;

    // To prevent two disposals running at the same time.
    private static readonly Lock _disposingLock = new();

    [STAThread]
    public static void Main()
    {
        // Start the application as a single instance
        if (SingleInstance<App>.InitializeAsFirstInstance())
        {
            using var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _mainWindow ??= new SettingWindow();
        _mainWindow.Show();

        Current.MainWindow = _mainWindow;
        Current.MainWindow.Title = Constants.FlowBar;

        var barWindow = new BarWindow();
        barWindow.Show();
    }

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

    public void OnSecondAppStarted()
    {

    }
}
