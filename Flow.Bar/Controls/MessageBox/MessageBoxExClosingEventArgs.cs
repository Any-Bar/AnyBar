using System;
using System.Windows;

namespace Flow.Bar.Controls;

public sealed class MessageBoxExClosingEventArgs : EventArgs
{
    private MessageBoxExClosingDeferral _deferral = null!;
    private int _deferralCount;

    internal MessageBoxExClosingEventArgs(MessageBoxResult result)
    {
        Result = result;
    }

    public bool Cancel { get; set; }

    public MessageBoxResult Result { get; }

    public MessageBoxExClosingDeferral GetDeferral()
    {
        _deferralCount++;

        return new MessageBoxExClosingDeferral(DecrementDeferralCount);
    }

    internal void SetDeferral(MessageBoxExClosingDeferral deferral)
    {
        _deferral = deferral;
    }

    internal void DecrementDeferralCount()
    {
        _deferralCount--;
        if (_deferralCount == 0)
        {
            _deferral.Complete();
        }
    }

    internal void IncrementDeferralCount()
    {
        _deferralCount++;
    }
}
