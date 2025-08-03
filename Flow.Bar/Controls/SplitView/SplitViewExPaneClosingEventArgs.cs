using System;

namespace Flow.Bar.Controls.SplitView;

public sealed class SplitViewPaneClosingEventArgs : EventArgs
{
    internal SplitViewPaneClosingEventArgs()
    {
    }

    public bool Cancel { get; set; }
}
