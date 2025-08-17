using System;
using System.Windows.Threading;
using WpfApplication = System.Windows.Application;

namespace Flow.Bar.Helpers.Dispatcher;

public static class DispatcherHelper
{
    public static void RunOnMainThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (WpfApplication.Current?.Dispatcher.CheckAccess() != true)
        {
            WpfApplication.Current?.Dispatcher.Invoke(action, priority);
        }

        action();
    }

    public static T? RunOnMainThread<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (WpfApplication.Current?.Dispatcher.CheckAccess() != true)
        {
            if (WpfApplication.Current == null)
            {
                return default;
            }

            return WpfApplication.Current.Dispatcher.Invoke(action, priority);
        }

        return action();
    }
}
