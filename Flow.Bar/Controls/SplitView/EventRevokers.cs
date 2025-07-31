using Flow.Bar.Controls.NavigationView;

namespace Flow.Bar.Controls.SplitView;

internal class SplitViewIsPaneOpenChangedRevoker(SplitView source, DependencyPropertyChangedCallback handler) : EventRevoker<SplitView, DependencyPropertyChangedCallback>(source, handler)
{
    protected override void AddHandler(SplitView source, DependencyPropertyChangedCallback handler)
    {
        source.IsPaneOpenChanged += handler;
    }

    protected override void RemoveHandler(SplitView source, DependencyPropertyChangedCallback handler)
    {
        source.IsPaneOpenChanged -= handler;
    }
}

internal class SplitViewDisplayModeChangedRevoker(SplitView source, DependencyPropertyChangedCallback handler) : EventRevoker<SplitView, DependencyPropertyChangedCallback>(source, handler)
{
    protected override void AddHandler(SplitView source, DependencyPropertyChangedCallback handler)
    {
        source.DisplayModeChanged += handler;
    }

    protected override void RemoveHandler(SplitView source, DependencyPropertyChangedCallback handler)
    {
        source.DisplayModeChanged -= handler;
    }
}

internal class SplitViewCompactPaneLengthChangedRevoker(SplitView source, DependencyPropertyChangedCallback handler) : EventRevoker<SplitView, DependencyPropertyChangedCallback>(source, handler)
{
    protected override void AddHandler(SplitView source, DependencyPropertyChangedCallback handler)
    {
        source.CompactPaneLengthChanged += handler;
    }

    protected override void RemoveHandler(SplitView source, DependencyPropertyChangedCallback handler)
    {
        source.CompactPaneLengthChanged -= handler;
    }
}
