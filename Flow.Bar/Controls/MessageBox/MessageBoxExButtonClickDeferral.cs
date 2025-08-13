using System;

namespace Flow.Bar.Controls;

public sealed class MessageBoxExButtonClickDeferral
{
    private readonly Action _handler;

    internal MessageBoxExButtonClickDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        _handler();
    }
}
