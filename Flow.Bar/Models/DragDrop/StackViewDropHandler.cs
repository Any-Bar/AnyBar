using System;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;

namespace Flow.Bar.Models.DragDrop;

public class StackViewDropHandler : DefaultDropHandler
{
    private static Type Insert { get; } = typeof(StackViewDropTargetInsertionAdorner);

    /// <inheritdoc />
    public override void DragOver(IDropInfo dropInfo)
    {
        if (CanAcceptData(dropInfo))
        {
            var copyData = ShouldCopyData(dropInfo);
            dropInfo.Effects = copyData ? DragDropEffects.Copy : DragDropEffects.Move;
            var isTreeViewItem = dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.VisualTargetItem is TreeViewItem;
            dropInfo.DropTargetAdorner = isTreeViewItem ? DropTargetAdorners.Highlight : Insert;

            dropInfo.DropTargetHintState = DropHintState.Active;
            dropInfo.DropTargetHintAdorner = DropTargetAdorners.Hint;
        }
        else
        {
            dropInfo.Effects = DragDropEffects.None;
            dropInfo.DropTargetHintAdorner = DropTargetAdorners.Hint;
            dropInfo.DropTargetHintState = DropHintState.Error;
        }
    }
}
