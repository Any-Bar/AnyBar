using System;
using System.Diagnostics;
using iNKORE.UI.WPF.Modern.Controls;

namespace AnyBar.Controls;

public sealed class ContentDialogExClosingEventArgs : EventArgs
{
    private ContentDialogExClosingDeferral _deferral = null!;
    private int _deferralCount;

    internal ContentDialogExClosingEventArgs(ContentDialogResult result)
    {
        Result = result;
    }

    public bool Cancel { get; set; }

    public ContentDialogResult Result { get; }

    public ContentDialogExClosingDeferral GetDeferral()
    {
        _deferralCount++;

        return new ContentDialogExClosingDeferral(() =>
        {
            DecrementDeferralCount();
        });
    }

    internal void SetDeferral(ContentDialogExClosingDeferral deferral)
    {
        _deferral = deferral;
    }

    internal void DecrementDeferralCount()
    {
        Debug.Assert(_deferralCount > 0);
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
