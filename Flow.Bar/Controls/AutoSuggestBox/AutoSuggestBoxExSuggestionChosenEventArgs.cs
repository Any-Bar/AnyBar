using System;

namespace Flow.Bar.Controls;

public sealed class AutoSuggestBoxExSuggestionChosenEventArgs : EventArgs
{
    public AutoSuggestBoxExSuggestionChosenEventArgs()
    {
    }

    public object SelectedItem { get; internal set; } = null!;
}
