using System;

namespace Flow.Bar.Controls;

public enum AutoSuggestionBoxTextChangeReason
{
    UserInput = 0,
    ProgrammaticChange = 1,
    SuggestionChosen = 2
}

public sealed class AutoSuggestBoxExTextChangedEventArgs : EventArgs
{
    public AutoSuggestBoxExTextChangedEventArgs()
    {
    }

    internal AutoSuggestBoxExTextChangedEventArgs(AutoSuggestBoxEx source, string value, AutoSuggestionBoxTextChangeReason reason)
    {
        m_source = new WeakReference<AutoSuggestBoxEx>(source);
        m_value = value;
        Reason = reason;
    }

    public AutoSuggestionBoxTextChangeReason Reason { get; private set; }

    public bool CheckCurrent()
    {
        return m_source != null &&
               m_source.TryGetTarget(out var source) &&
               source.Text == m_value;
    }

    private readonly WeakReference<AutoSuggestBoxEx>? m_source;
    private readonly string? m_value;
}
