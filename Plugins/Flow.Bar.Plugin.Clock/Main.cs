using Flow.Bar.Plugin.Clock.Views;
using System.Windows;

namespace Flow.Bar.Plugin.Clock;

public class Main : IPlugin
{
    internal static PluginInitContext Context { get; private set; }

    public void Init(PluginInitContext context)
    {
        Context = context;
    }

    public FrameworkElement GetBarElement()
    {
        return new ClockControl();
    }
}
