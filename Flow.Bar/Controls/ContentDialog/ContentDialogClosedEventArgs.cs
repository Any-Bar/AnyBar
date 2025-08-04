using System;

namespace Flow.Bar.Controls;

public class ContentDialogClosedEventArgs : EventArgs
{
    internal ContentDialogClosedEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public ContentDialogResult Result { get; }
}
