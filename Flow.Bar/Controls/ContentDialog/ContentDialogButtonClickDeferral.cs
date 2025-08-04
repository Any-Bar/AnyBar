using System;

namespace Flow.Bar.Controls;

public sealed class ContentDialogButtonClickDeferral
{
    private readonly Action? _handler;

    internal ContentDialogButtonClickDeferral(Action? handler)
    {
        _handler = handler;
    }

    public void Complete()
    {
        _handler?.Invoke();
    }
}
