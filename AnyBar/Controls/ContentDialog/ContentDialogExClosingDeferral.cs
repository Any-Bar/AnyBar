using System;

namespace AnyBar.Controls;

public sealed class ContentDialogExClosingDeferral
{
    private readonly Action _handler;

    internal ContentDialogExClosingDeferral(Action handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Complete()
    {
        _handler();
    }
}
