using System;

namespace Flow.Bar.Controls;

public sealed class SplitViewPaneClosingEventArgs : EventArgs
{
    internal SplitViewPaneClosingEventArgs()
    {
    }

    public bool Cancel { get; set; }
}
