using System;
using System.ComponentModel;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Controls;

internal class SplitViewIsPaneOpenChangedRevoker(SplitView source, EventHandler handler) : EventRevoker<SplitView, EventHandler>(source, handler)
{
    protected override void AddHandler(SplitView source, EventHandler handler)
    {
        DependencyPropertyDescriptor.FromProperty(SplitView.IsPaneOpenProperty, typeof(SplitView))
            .AddValueChanged(source, handler);
    }

    protected override void RemoveHandler(SplitView source, EventHandler handler)
    {
        DependencyPropertyDescriptor.FromProperty(SplitView.IsPaneOpenProperty, typeof(SplitView))
            .RemoveValueChanged(source, handler);
    }
}

internal class SplitViewDisplayModeChangedRevoker(SplitView source, EventHandler handler) : EventRevoker<SplitView, EventHandler>(source, handler)
{
    protected override void AddHandler(SplitView source, EventHandler handler)
    {
        DependencyPropertyDescriptor.FromProperty(SplitView.DisplayModeProperty, typeof(SplitView))
            .AddValueChanged(source, handler);
    }

    protected override void RemoveHandler(SplitView source, EventHandler handler)
    {
        DependencyPropertyDescriptor.FromProperty(SplitView.DisplayModeProperty, typeof(SplitView))
            .RemoveValueChanged(source, handler);
    }
}

internal class SplitViewCompactPaneLengthChangedRevoker(SplitView source, EventHandler handler) : EventRevoker<SplitView, EventHandler>(source, handler)
{
    protected override void AddHandler(SplitView source, EventHandler handler)
    {
        DependencyPropertyDescriptor.FromProperty(SplitView.CompactPaneLengthProperty, typeof(SplitView))
            .AddValueChanged(source, handler);
    }

    protected override void RemoveHandler(SplitView source, EventHandler handler)
    {
        DependencyPropertyDescriptor.FromProperty(SplitView.CompactPaneLengthProperty, typeof(SplitView))
            .RemoveValueChanged(source, handler);
    }
}
