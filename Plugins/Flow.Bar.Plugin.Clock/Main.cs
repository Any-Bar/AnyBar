using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Flow.Bar.Plugin.Clock.Views;

namespace Flow.Bar.Plugin.Clock;

public class Main : IPlugin, IPluginI18n, IRightClickMenu
{
    private static readonly string ClassName = nameof(Main);

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

    public IList<MenuItem> GetRightClickMenuItems()
    {
        var menuItems = new List<MenuItem>();
        var adjustDateTimeItem = new MenuItem();
        adjustDateTimeItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, "FlowBarPlugin_Clock_RightClickMenu_AdjustDateTime");
        adjustDateTimeItem.Click += AdjustDateTimeItem_Click;
        menuItems.Add(adjustDateTimeItem);
        return menuItems;
    }

    private void AdjustDateTimeItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:dateandtime",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Context.API.ShowMsgError(Context.API.GetTranslation("FlowBarPlugin_Clock_RightClickMenu_FailToOpenDateTime"));
            Context.API.LogFatal(ClassName, "Failed to open Date and Time settings", ex);
        }
    }
}
