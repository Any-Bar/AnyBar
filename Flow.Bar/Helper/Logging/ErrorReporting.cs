using System;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Flow.Bar.Helper.Logging;

public static class ErrorReporting
{
    private static readonly string ClassName = nameof(ErrorReporting);

    private static void Report(Exception e, [CallerMemberName] string methodName = "UnHandledException")
    {
        App.API.LogFatal(ClassName, ExceptionFormatter.FormatExcpetion(e), e, methodName);
        // TODO: Add ReportWindow
        /*var reportWindow = new ReportWindow(e);
        reportWindow.Show();*/
    }

    public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // handle non-ui thread exceptions
        Report((Exception)e.ExceptionObject);
    }

    public static void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // handle ui thread exceptions
        Report(e.Exception);
        // prevent application exist, so the user can copy prompted error info
        e.Handled = true;
    }
}
