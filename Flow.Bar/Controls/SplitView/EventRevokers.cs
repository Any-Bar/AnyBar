namespace Flow.Bar.Controls;

internal class SplitViewIsPaneOpenChangedRevoker(SplitViewEx source, DependencyPropertyChangedCallback handler) : EventRevoker<SplitViewEx, DependencyPropertyChangedCallback>(source, handler)
{
    protected override void AddHandler(SplitViewEx source, DependencyPropertyChangedCallback handler)
    {
        source.IsPaneOpenChanged += handler;
    }

    protected override void RemoveHandler(SplitViewEx source, DependencyPropertyChangedCallback handler)
    {
        source.IsPaneOpenChanged -= handler;
    }
}

internal class SplitViewDisplayModeChangedRevoker(SplitViewEx source, DependencyPropertyChangedCallback handler) : EventRevoker<SplitViewEx, DependencyPropertyChangedCallback>(source, handler)
{
    protected override void AddHandler(SplitViewEx source, DependencyPropertyChangedCallback handler)
    {
        source.DisplayModeChanged += handler;
    }

    protected override void RemoveHandler(SplitViewEx source, DependencyPropertyChangedCallback handler)
    {
        source.DisplayModeChanged -= handler;
    }
}

internal class SplitViewCompactPaneLengthChangedRevoker(SplitViewEx source, DependencyPropertyChangedCallback handler) : EventRevoker<SplitViewEx, DependencyPropertyChangedCallback>(source, handler)
{
    protected override void AddHandler(SplitViewEx source, DependencyPropertyChangedCallback handler)
    {
        source.CompactPaneLengthChanged += handler;
    }

    protected override void RemoveHandler(SplitViewEx source, DependencyPropertyChangedCallback handler)
    {
        source.CompactPaneLengthChanged -= handler;
    }
}
