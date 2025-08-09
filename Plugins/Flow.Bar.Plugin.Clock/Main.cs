using Flow.Bar.Plugin.Clock.Views;
using System.Windows;

namespace Flow.Bar.Plugin.Clock;

public class Main : IPlugin, IPluginI18n
{
    internal static PluginInitContext Context { get; private set; }

    public void Init(PluginInitContext context)
    {
        Context = context;
    }

    public FrameworkElement GetBarElement(BarElementPosition position)
    {
        return new ClockControl(position);
    }

    public string GetTranslatedPluginTitle()
    {
        return Context.API.GetTranslation("FlowBarPlugin_Clock_PluginName");
    }

    public string GetTranslatedPluginDescription()
    {
        return Context.API.GetTranslation("FlowBarPlugin_Clock_PluginDescription");
    }
}
