using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AnyBar.Controls;
using AnyBar.Models.DragDrop;
using GongSolutions.Wpf.DragDrop;
using iNKORE.UI.WPF.Modern;
using WpfApplication = System.Windows.Application;
using WpfDragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace AnyBar.Helpers.DragDrop;

public static class DragDropHelper
{
    #region CanReorderItems

    public static readonly DependencyProperty CanReorderItemsProperty =
        DependencyProperty.RegisterAttached(
            "CanReorderItems",
            typeof(bool),
            typeof(DragDropHelper),
            new FrameworkPropertyMetadata(false, OnCanReorderItemsPropertyChanged));

    private static void OnCanReorderItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.OldValue)
        {
            d.DisableDragDrop();
        }
        if ((bool)e.NewValue)
        {
            d.EnableDragDrop();
            if (d.GetType() == typeof(StackView))
            {
                d.SetStackViewDropHandler();
            }
        }
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static bool GetCanReorderItems(DependencyObject d)
    {
        return (bool)d.GetValue(CanReorderItemsProperty);
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static void SetCanReorderItems(DependencyObject d, bool value)
    {
        d.SetValue(CanReorderItemsProperty, value);
    }

    #endregion

    #region DropScrollViewer

    public static readonly DependencyProperty DropScrollViewerProperty =
        DependencyProperty.RegisterAttached(
            "DropScrollViewer",
            typeof(ScrollViewer),
            typeof(DragDropHelper),
            new FrameworkPropertyMetadata(null, OnDropScrollViewerPropertyChanged));

    private static void OnDropScrollViewerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ScrollViewer scrollViewer)
        {
            d.ApplyDropScrollViewerConfiguration(scrollViewer);
        }
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static ScrollViewer? GetDropScrollViewer(DependencyObject d)
    {
        return d.GetValue(DropScrollViewerProperty) is ScrollViewer sv ? sv : null;
    }

    [AttachedPropertyBrowsableForType(typeof(UIElement))]
    public static void SetDropScrollViewer(DependencyObject d, ScrollViewer value)
    {
        d.SetValue(DropScrollViewerProperty, value);
    }

    #endregion

    #region Private Fields & Methods

    private static readonly DataTemplate _emptyEffectAdornerTemplate = new();
    private static readonly StackViewDropHandler _stackViewDropHandler = new();

    private static void EnableDragDrop(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);

        WpfDragDrop.SetIsDragSource(element, true);
        WpfDragDrop.SetIsDropTarget(element, true);
        WpfDragDrop.SetUseDefaultEffectDataTemplate(element, false);
        WpfDragDrop.SetEffectAllAdornerTemplate(element, _emptyEffectAdornerTemplate);
        WpfDragDrop.SetEffectCopyAdornerTemplate(element, _emptyEffectAdornerTemplate);
        WpfDragDrop.SetEffectLinkAdornerTemplate(element, _emptyEffectAdornerTemplate);
        WpfDragDrop.SetEffectMoveAdornerTemplate(element, _emptyEffectAdornerTemplate);
        WpfDragDrop.SetEffectNoneAdornerTemplate(element, _emptyEffectAdornerTemplate);
        WpfDragDrop.SetEffectScrollAdornerTemplate(element, _emptyEffectAdornerTemplate);
    }
    
    private static void DisableDragDrop(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);

        WpfDragDrop.SetIsDragSource(element, false);
        WpfDragDrop.SetIsDropTarget(element, false);
    }

    private static void ApplyDropScrollViewerConfiguration(this DependencyObject element, ScrollViewer scrollViewer)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(scrollViewer);

        WpfDragDrop.SetDropTargetScrollViewer(element, scrollViewer);
        WpfDragDrop.SetDropTargetAdornerBrush(element, WpfApplication.Current.TryFindResource(ThemeKeys.AccentFillColorDefaultBrushKey) as Brush);
        var horizontalScrollBarEnabled = scrollViewer.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible ||
            scrollViewer.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto;
        var verticalScrollBarEnabled = scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Visible ||
            scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto;
        if (horizontalScrollBarEnabled)
        {
            if (verticalScrollBarEnabled)
            {
                WpfDragDrop.SetDropScrollingMode(element, ScrollingMode.Both);
            }
            else
            {
                WpfDragDrop.SetDropScrollingMode(element, ScrollingMode.HorizontalOnly);
            }
        }
        else
        {
            if (verticalScrollBarEnabled)
            {
                WpfDragDrop.SetDropScrollingMode(element, ScrollingMode.VerticalOnly);
            }
            else
            {
                WpfDragDrop.SetDropScrollingMode(element, ScrollingMode.None);
            }
        }
    }

    

    private static void SetStackViewDropHandler(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);

        WpfDragDrop.SetDropHandler(element, _stackViewDropHandler);
    }

    #endregion
}
