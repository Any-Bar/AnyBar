using System;

namespace Flow.Bar.Controls;

public sealed class ContentDialogExButtonClickDeferral
{
    private readonly Action? _handler;

    internal ContentDialogExButtonClickDeferral(Action? handler)
    {
        _handler = handler;
    }

    public void Complete()
    {
        _handler?.Invoke();
    }
}
