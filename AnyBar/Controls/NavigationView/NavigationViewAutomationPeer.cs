// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace AnyBar.Controls;

internal class NavigationViewAutomationPeer(NavigationView owner) : FrameworkElementAutomationPeer(owner), ISelectionProvider
{
    public override object GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.Selection)
        {
            return this;
        }

        return base.GetPattern(patternInterface);
    }

    public bool CanSelectMultiple => false;

    public bool IsSelectionRequired => false;

    public IRawElementProviderSimple[] GetSelection()
    {
        if (Owner is NavigationView nv)
        {
            if (nv.GetSelectedContainer() is { } nvi)
            {
                if (CreatePeerForElement(nvi) is { } peer)
                {
                    return [ProviderFromPeer(peer)];
                }
            }
        }
        return [];
    }

    internal void RaiseSelectionChangedEvent(object oldSelection, object newSelecttion)
    {
        if (ListenerExists(AutomationEvents.SelectionPatternOnInvalidated))
        {
            if (Owner is NavigationView nv)
            {
                if (nv.GetSelectedContainer() is { } nvi)
                {
                    if (CreatePeerForElement(nvi) is { } peer)
                    {
                        peer.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
                    }
                }
            }
        }
    }
}
