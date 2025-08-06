using iNKORE.UI.WPF.Modern;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfDragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Flow.Bar.Helper.DragDrop;

public static class DragDropHelper
{
    private readonly static DataTemplate _emptyEffectAdornerTemplate = new();

    public static void EnableDragDrop(this DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));

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
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(scrollViewer, nameof(scrollViewer));

        WpfDragDrop.SetDropTargetScrollViewer(element, scrollViewer);
        WpfDragDrop.SetDropTargetAdornerBrush(element, System.Windows.Application.Current.TryFindResource(ThemeKeys.AccentFillColorDefaultBrushKey) as Brush);
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
        ArgumentNullException.ThrowIfNull(element, nameof(element));

        WpfDragDrop.SetIsDragSource(element, false);
        WpfDragDrop.SetIsDropTarget(element, false);
    }
}
