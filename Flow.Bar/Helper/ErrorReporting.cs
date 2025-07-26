using Microsoft.Win32;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Flow.Bar.Helper;

public static class ErrorReporting
{
    private static void Report(Exception e, bool silent = false, [CallerMemberName] string methodName = "UnHandledException")
    {
        /*var logger = LogManager.GetLogger(methodName);
        logger.Fatal(ExceptionFormatter.FormatExcpetion(e));
        if (silent) return;
        var reportWindow = new ReportWindow(e);
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

    public static string RuntimeInfo()
    {
        var info =
            $"""

             Flow Launcher version: {Constants.Version}
             OS Version: {GetWindowsFullVersionFromRegistry()}
             IntPtr Length: {IntPtr.Size}
             x64: {Environment.Is64BitOperatingSystem}
             """;
        return info;
    }

    private static string GetWindowsFullVersionFromRegistry()
    {
        try
        {
            var buildRevision = GetWindowsRevisionFromRegistry();
            var currentBuild = Environment.OSVersion.Version.Build;
            return currentBuild.ToString() + "." + buildRevision;
        }
        catch
        {
            return Environment.OSVersion.VersionString;
        }
    }

    private static string GetWindowsRevisionFromRegistry()
    {
        try
        {
            using var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\");
            var buildRevision = registryKey?.GetValue("UBR")?.ToString();
            return buildRevision ?? throw new ArgumentNullException();
        }
        catch
        {
            return "0";
        }
    }
}
