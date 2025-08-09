using System;

namespace Flow.Bar.Controls;

public sealed class AutoSuggestBoxExQuerySubmittedEventArgs : EventArgs
{
    public AutoSuggestBoxExQuerySubmittedEventArgs()
    {
    }

    public object? ChosenSuggestion { get; internal set; }
    public string QueryText { get; internal set; } = string.Empty;
}
