using System;
using System.Windows;

namespace Flow.Bar.Controls;

public class MessageBoxExClosedEventArgs : EventArgs
{
    internal MessageBoxExClosedEventArgs(MessageBoxResult result)
    {
        Result = result;
    }

    public MessageBoxResult Result { get; }
}
