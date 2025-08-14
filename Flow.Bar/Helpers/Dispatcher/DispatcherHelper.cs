using System;
using System.Windows.Threading;

namespace Flow.Bar.Helpers.Dispatcher;

public static class DispatcherHelper
{
    public static void RunOnMainThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() != true)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(action, priority);
        }

        action();
    }

    public static T? RunOnMainThread<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() != true)
        {
            if (System.Windows.Application.Current == null)
            {
                return default;
            }

            return System.Windows.Application.Current.Dispatcher.Invoke(action, priority);
        }

        return action();
    }
}
