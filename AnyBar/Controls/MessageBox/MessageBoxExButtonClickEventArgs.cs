using System;

namespace AnyBar.Controls;

public class MessageBoxExButtonClickEventArgs : EventArgs
{
    private MessageBoxExButtonClickDeferral _deferral = null!;
    private int _deferralCount;

    internal MessageBoxExButtonClickEventArgs()
    {
    }

    public bool Cancel { get; set; }

    public MessageBoxExButtonClickDeferral GetDeferral()
    {
        _deferralCount++;

        return new MessageBoxExButtonClickDeferral(DecrementDeferralCount);
    }

    internal void SetDeferral(MessageBoxExButtonClickDeferral deferral)
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
