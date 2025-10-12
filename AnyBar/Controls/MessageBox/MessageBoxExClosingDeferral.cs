using System;

namespace AnyBar.Controls;

public sealed class MessageBoxExClosingDeferral
{
    private readonly Action _handler;

    internal MessageBoxExClosingDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        _handler();
    }
}
