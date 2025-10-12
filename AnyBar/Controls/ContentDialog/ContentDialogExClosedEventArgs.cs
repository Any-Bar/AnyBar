using System;
using iNKORE.UI.WPF.Modern.Controls;

namespace AnyBar.Controls;

public class ContentDialogExClosedEventArgs : EventArgs
{
    internal ContentDialogExClosedEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public ContentDialogResult Result { get; }
}
