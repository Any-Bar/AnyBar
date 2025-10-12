using System;

namespace AnyBar.Controls;

public class ContentDialogExButtonClickEventArgs : EventArgs
{
    private ContentDialogExButtonClickDeferral _deferral = null!;
    private int _deferralCount;

    internal ContentDialogExButtonClickEventArgs()
    {
    }

    public bool Cancel { get; set; }

    public ContentDialogExButtonClickDeferral GetDeferral()
    {
        _deferralCount++;

        return new ContentDialogExButtonClickDeferral(DecrementDeferralCount);
    }

    internal void SetDeferral(ContentDialogExButtonClickDeferral deferral)
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
