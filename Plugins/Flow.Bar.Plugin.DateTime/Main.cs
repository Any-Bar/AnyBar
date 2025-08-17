using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Flow.Bar.Plugin.DateTime.Views;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Plugin.DateTime;

public class Main : IPlugin, IPluginI18n, ICustomLeftClickMenu, IRightClickMenu, ILeftClick, IRightClick
{
    private static readonly string ClassName = nameof(Main);

    internal static PluginInitContext Context { get; private set; }

    private readonly ConcurrentDictionary<string, DateTimeControl> _runningBarElements = new();

    public void Init(PluginInitContext context)
    {
        Context = context;
    }

    public FrameworkElement CreateBarElement(BarElementContext context)
    {
        Context.API.LogVerbose(ClassName, $"Creating DateTime control for position: {context.Position}");

        if (_runningBarElements.TryGetValue(context.Id, out var existingControl))
        {
            existingControl.OnDockModeChanged(context.Position);
            return existingControl;
        }

        var control = new DateTimeControl(context.Position);
        _runningBarElements.TryAdd(context.Id, control);

        return control;
    }

    public void DeleteBarElement(string id)
    {
        Context.API.LogVerbose(ClassName, $"Deleting DateTime control for id: {id}");

        if (_runningBarElements.TryGetValue(id, out var _))
        {
            _runningBarElements.TryRemove(id, out _);
        }
    }

    public void OnBarElementContextChanged(BarElementContextChangedAgrs args)
    {
        Context.API.LogVerbose(ClassName, $"Bar element context changed for id: {args.Context.Id}");

        if (_runningBarElements.TryGetValue(args.Context.Id, out var existingControl))
        {
            existingControl.OnDockModeChanged(args.Context.Position);
        }
    }

    public string GetTranslatedPluginTitle()
    {
        return Localize.FlowBarPlugin_DateTime_PluginName();
    }

    public string GetTranslatedPluginDescription()
    {
        return Localize.FlowBarPlugin_DateTime_PluginDescription();
    }

    public ContextMenuPopupMode GetLeftClickMenuPopupMode(BarElementContext context)
    {
        return ContextMenuPopupMode.PopupAndFadeAway;
    }

    public Style GetLeftClickMenuMenuStyle(BarElementContext context)
    {
        Context.API.LogVerbose(ClassName, "Retrieving left-click menu style for DateTime control");

        return (Style)Application.Current.Resources["FlowBarPlugin_DateTime_LeftClickMenuStyle"];
    }

    public void OnApplyLeftClickMenuTemplate(BarElementContext context, ContextMenu menu)
    {

    }

    public ContextMenuPopupMode GetRightClickMenuPopupMode(BarElementContext context)
    {
        return ContextMenuPopupMode.AlwaysPopup;
    }

    public IList<MenuItem> GetRightClickMenuItems(BarElementContext context)
    {
        Context.API.LogVerbose(ClassName, "Retrieving right-click menu items for DateTime control");

        var menuItems = new List<MenuItem>();
        var adjustDateTimeItem = new MenuItem()
        {
            Icon = new FontIcon { Glyph = "\ue713" }
        };
        adjustDateTimeItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.FlowBarPlugin_DateTime_RightClickMenu_AdjustDateTime));
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
            Context.API.ShowMsgError(Localize.FlowBarPlugin_DateTime_RightClickMenu_FailToOpenDateTime());
            Context.API.LogFatal(ClassName, "Failed to open Date and Time settings", ex);
        }
    }

    public void OnMouseLeftButtonDown(BarElementContext context, object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Left mouse button down event invoked");
    }

    public void OnMouseLeftButtonUp(BarElementContext context, object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Left mouse button up event invoked");
    }

    public void OnMouseRightButtonDown(BarElementContext context, object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Right mouse button down event invoked");
    }

    public void OnMouseRightButtonUp(BarElementContext context, object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Right mouse button up event invoked");
    }
}
