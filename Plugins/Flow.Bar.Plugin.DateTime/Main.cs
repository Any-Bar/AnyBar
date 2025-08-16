using System;
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

    public void Init(PluginInitContext context)
    {
        Context = context;
    }

    public FrameworkElement GetBarElement(BarElementPosition position)
    {
        Context.API.LogVerbose(ClassName, $"Creating DateTime control for position: {position}");
        return new DateTimeControl(position);
    }

    public string GetTranslatedPluginTitle()
    {
        return Localize.FlowBarPlugin_DateTime_PluginName();
    }

    public string GetTranslatedPluginDescription()
    {
        return Localize.FlowBarPlugin_DateTime_PluginDescription();
    }

    public ContextMenuPopupMode LeftClickMenuPopupMode => ContextMenuPopupMode.PopupAndFadeAway;

    public Style GetLeftClickMenuMenuStyle()
    {
        return (Style)Application.Current.Resources["FlowBarPlugin_DateTime_LeftClickMenuStyle"];
    }

    public void OnApplyLeftClickMenuTemplate(ContextMenu menu)
    {

    }

    public ContextMenuPopupMode RightClickMenuPopupMode => ContextMenuPopupMode.AlwaysPopup;

    public IList<MenuItem> GetRightClickMenuItems()
    {
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

    public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Left mouse button down event invoked");
    }

    public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Left mouse button up event invoked");
    }

    public void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Right mouse button down event invoked");
    }

    public void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        Context.API.LogVerbose(ClassName, "Right mouse button up event invoked");
    }
}
