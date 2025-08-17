using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;
using WpfDragDrop = GongSolutions.Wpf.DragDrop.DragDrop;
using WpfApplication = System.Windows.Application;

namespace Flow.Bar.Helpers.DragDrop;

public static class DragDropHelper
{
    private static readonly DataTemplate _emptyEffectAdornerTemplate = new();

    public static void EnableDragDrop(this DependencyObject element)
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

    public static void SetDropScrollViewer(this DependencyObject element, ScrollViewer scrollViewer)
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
                WpfDragDrop.SetDropScrollingMode(element, GongSolutions.Wpf.DragDrop.ScrollingMode.Both);
            }
            else
            {
                WpfDragDrop.SetDropScrollingMode(element, GongSolutions.Wpf.DragDrop.ScrollingMode.HorizontalOnly);
            }
        }
        else
        {
            if (verticalScrollBarEnabled)
            {
                WpfDragDrop.SetDropScrollingMode(element, GongSolutions.Wpf.DragDrop.ScrollingMode.VerticalOnly);
            }
            else
            {
                WpfDragDrop.SetDropScrollingMode(element, GongSolutions.Wpf.DragDrop.ScrollingMode.None);
            }
        }
    }

    public static void DisableDragDrop(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);

        WpfDragDrop.SetIsDragSource(element, false);
        WpfDragDrop.SetIsDropTarget(element, false);
    }
}
