using System;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Controls;

public class ContentDialogExClosedEventArgs : EventArgs
{
    internal ContentDialogExClosedEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public ContentDialogResult Result { get; }
}
