// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using iNKORE.UI.WPF.Modern.Media.Animation;

namespace AnyBar.Controls;

public sealed class NavigationViewItemInvokedEventArgs : EventArgs
{
    public NavigationViewItemInvokedEventArgs()
    {
    }

    public required object InvokedItem { get; set; }

    public required NavigationViewItemBase? InvokedItemContainer { get; set; }
    public required NavigationTransitionInfo RecommendedNavigationTransitionInfo { get; set; }
}
