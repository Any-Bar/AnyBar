// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using iNKORE.UI.WPF.Modern.Controls;

namespace AnyBar.Controls;

public class NavigationViewItemAutomationPeer(NavigationViewItem owner) :
    FrameworkElementAutomationPeer(owner),
    IInvokeProvider,
    ISelectionItemProvider
{
    protected override string GetNameCore()
    {
        var returnHString = base.GetNameCore();

        // If a name hasn't been provided by AutomationProperties.Name in markup:
        if (string.IsNullOrEmpty(returnHString))
        {
            if (Owner is NavigationViewItem lvi)
            {
                returnHString = TryGetStringRepresentationFromObject(lvi.Content);
            }
        }

        return returnHString;
    }

    private static string TryGetStringRepresentationFromObject(object obj)
    {
        return obj?.ToString() ?? string.Empty;
    }

    public override object GetPattern(PatternInterface pattern)
    {
        if (pattern == PatternInterface.SelectionItem)
        {
            return this;
        }

        return base.GetPattern(pattern);
    }

    protected override string GetClassNameCore()
    {
        return nameof(NavigationViewItem);
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.ListItem;
    }

    protected override int GetPositionInSetCore()
    {
        return GetPositionOrSetCountInLeftNavHelper(AutomationOutput.Position);
    }

    protected override int GetSizeOfSetCore()
    {
        return GetPositionOrSetCountInLeftNavHelper(AutomationOutput.Size);
    }

    // Get either the position or the size of the set for this particular item in the case of left nav. 
    // We go through all the items and then we determine if the listviewitem from the left listview can be a navigation view item header
    // or a navigation view item. If it's the former, we just reset the count. If it's the latter, we increment the counter.
    // In case of calculating the position, if this is the NavigationViewItemAutomationPeer we're iterating through we break the loop.
    private int GetPositionOrSetCountInLeftNavHelper(AutomationOutput automationOutput)
    {
        var returnValue = 0;

        if (GetParentItemsRepeater() is { } repeater)
        {
            if (FrameworkElementAutomationPeer.CreatePeerForElement(repeater) is AutomationPeer parent)
            {
                if (parent.GetChildren() is { } children)
                {
                    var index = 0;

                    foreach (var child in children)
                    {
                        if (repeater.TryGetElement(index) is { } dependencyObject)
                        {
                            if (dependencyObject is NavigationViewItem navviewItem)
                            {
                                if (navviewItem.Visibility == System.Windows.Visibility.Visible)
                                {
                                    returnValue++;

                                    if (FrameworkElementAutomationPeer.FromElement(navviewItem) == (this))
                                    {
                                        if (automationOutput == AutomationOutput.Position)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        index++;
                    }
                }
            }
        }

        return returnValue;
    }

    private ItemsRepeater? GetParentItemsRepeater()
    {
        if (GetParentNavigationView() is { })
        {
            if (Owner is NavigationViewItemBase navigationViewItem)
            {
                return NavigationView.GetParentItemsRepeaterForContainer(navigationViewItem);
            }
        }
        return null;
    }

    void IInvokeProvider.Invoke()
    {
        if (GetParentNavigationView() is { } navView)
        {
            if (Owner is NavigationViewItem navigationViewItem)
            {
                navView.OnNavigationViewItemInvoked(navigationViewItem);
            }
        }
    }

    private NavigationView? GetParentNavigationView()
    {
        NavigationView? navigationView = null;

        if (Owner is NavigationViewItemBase navigationViewItem)
        {
            navigationView = navigationViewItem.GetNavigationView();
        }
        return navigationView;
    }

    bool ISelectionItemProvider.IsSelected
    {
        get
        {
            if (Owner is NavigationViewItem nvi)
            {
                return nvi.IsSelected;
            }
            return false;
        }
    }

    IRawElementProviderSimple? ISelectionItemProvider.SelectionContainer
    {
        get
        {
            if (GetParentNavigationView() is { } navview)
            {
                if (CreatePeerForElement(navview) is { } peer)
                {
                    return ProviderFromPeer(peer);
                }
            }

            return null;
        }
    }

    void ISelectionItemProvider.AddToSelection()
    {
        ChangeSelection(true);
    }

    void ISelectionItemProvider.Select()
    {
        ChangeSelection(true);
    }

    void ISelectionItemProvider.RemoveFromSelection()
    {
        ChangeSelection(false);
    }

    private void ChangeSelection(bool isSelected)
    {
        if (Owner is NavigationViewItem nvi)
        {
            nvi.IsSelected = isSelected;
        }
    }

    internal enum AutomationOutput
    {
        Position,
        Size,
    }
}
