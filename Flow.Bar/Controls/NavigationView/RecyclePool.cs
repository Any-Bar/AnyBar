// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace Flow.Bar.Controls;

public class RecyclePool
{
    public void PutElement(
        UIElement element,
        string key)
    {
        PutElementCore(element, key, null /* owner */);
    }

    public void PutElement(
        UIElement element,
        string key,
        UIElement owner)
    {
        PutElementCore(element, key, owner);
    }

    public UIElement? TryGetElement(
        string key)
    {
        return TryGetElementCore(key, null /* owner */);
    }

    public UIElement? TryGetElement(
        string key,
        UIElement? owner)
    {
        return TryGetElementCore(key, owner);
    }

    protected virtual void PutElementCore(
        UIElement element,
        string key,
        UIElement? owner)
    {
        var winrtKey = key;
        var winrtOwner = owner;
        var winrtOwnerAsPanel = EnsureOwnerIsPanelOrNull(winrtOwner);

        ElementInfo elementInfo = new(element, winrtOwnerAsPanel);

        if (m_elements.TryGetValue(winrtKey, out var elements))
        {
            elements.Add(elementInfo);
        }
        else
        {
            List<ElementInfo> pool = [elementInfo];
            m_elements.Add(winrtKey, pool);
        }
    }

    protected virtual UIElement? TryGetElementCore(
        string key,
        UIElement? owner)
    {
        if (m_elements.TryGetValue(key, out var elements))
        {
            if (elements.Count > 0)
            {
                ElementInfo elementInfo = new(null, null);
                // Prefer an element from the same owner or with no owner so that we don't incur
                // the enter/leave cost during recycling.
                var winrtOwner = owner;
                var index = elements.FindIndex(elemInfo => elemInfo.Owner == winrtOwner || elemInfo.Owner == null);

                if (index >= 0)
                {
                    elementInfo = elements[index];
                    elements.RemoveAt(index); // elements.erase(iter);
                }
                else
                {
                    elementInfo = elements.Last();
                    elements.RemoveLast();
                }

                var ownerAsPanel = EnsureOwnerIsPanelOrNull(winrtOwner);
                if (elementInfo.Owner != null && elementInfo.Owner != ownerAsPanel)
                {
                    // Element is still under its parent. remove it from its parent.
                    var panel = elementInfo.Owner;
                    if (panel != null)
                    {
                        var childIndex = panel.Children.IndexOf(elementInfo.Element);
                        var found = childIndex >= 0;
                        if (!found)
                        {
                            throw new Exception($"{nameof(panel)}'s child not found in its {nameof(panel.Children)}");
                        }

                        panel.Children.RemoveAt(childIndex);
                    }
                }

                return elementInfo.Element;
            }
        }

        return null;
    }

    #region Properties

    internal static readonly DependencyProperty ReuseKeyProperty =
        DependencyProperty.RegisterAttached(
            "ReuseKey",
            typeof(string),
            typeof(RecyclePool),
            new PropertyMetadata(string.Empty));

    internal static string GetReuseKey(UIElement element)
    {
        return (string)element.GetValue(ReuseKeyProperty);
    }

    internal static void SetReuseKey(UIElement element, string value)
    {
        element.SetValue(ReuseKeyProperty, value);
    }

    private static readonly AttachableMemberIdentifier PoolInstanceProperty =
        new(typeof(RecyclePool), "PoolInstance");

    public static RecyclePool GetPoolInstance(DataTemplate? dataTemplate)
    {
        AttachablePropertyServices.TryGetProperty<RecyclePool>(dataTemplate, PoolInstanceProperty, out var value);
        return value;
    }

    public static void SetPoolInstance(DataTemplate? dataTemplate, RecyclePool value)
    {
        AttachablePropertyServices.SetProperty(dataTemplate, PoolInstanceProperty, value);
    }

    internal static readonly DependencyProperty OriginTemplateProperty =
        DependencyProperty.RegisterAttached(
            "OriginTemplate",
            typeof(DataTemplate),
            typeof(RecyclePool),
            null);

    #endregion

    private static Panel? EnsureOwnerIsPanelOrNull(UIElement? owner)
    {
        Panel? ownerAsPanel = null;
        if (owner != null)
        {
            ownerAsPanel = owner as Panel;
            if (ownerAsPanel == null)
            {
                throw new ArgumentException($"{nameof(owner)} must to be a {nameof(Panel)} or null");
            }
        }

        return ownerAsPanel;
    }

    private class ElementInfo(UIElement? element, Panel? owner)
    {
        public UIElement? Element { get; } = element;
        public Panel? Owner { get; } = owner;
    }

    private readonly Dictionary<string, List<ElementInfo>> m_elements = [];
}
