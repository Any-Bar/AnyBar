using Serilog;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Flow.Bar.Helper.Logging;

public static class ErrorReporting
{
    private static void Report(Exception e, [CallerMemberName] string methodName = "UnHandledException")
    {
        var logger = Log.ForContext("SourceContext", methodName);
        logger.Fatal(e, ExceptionFormatter.FormatExcpetion(e));
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
