using Flow.Bar.Models.Monitor;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Flow.Bar.Controls;

internal static class MenuFlyoutExPlacementHelper
{
    #region Placement

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.RegisterAttached(
            "Placement",
            typeof(MenuFlyoutExPlacementMode),
            typeof(MenuFlyoutExPlacementHelper),
            new PropertyMetadata(MenuFlyoutExPlacementMode.AppBarTop));

    public static MenuFlyoutExPlacementMode GetPlacement(DependencyObject element)
    {
        return (MenuFlyoutExPlacementMode)element.GetValue(PlacementProperty);
    }

    public static void SetPlacement(DependencyObject element, MenuFlyoutExPlacementMode value)
    {
        element.SetValue(PlacementProperty, value);
    }

    #endregion

    internal static CustomPopupPlacement[] PositionPopup(
        MenuFlyoutExPlacementMode placement,
        Size popupSize,
        Size targetSize,
        MonitorInfo monitor,
        Point? cursor,
        Point offset,
        FrameworkElement target,
        FrameworkElement? child = null)
    {
        Matrix transformToDevice = default;
        if (child != null)
        {
            TryGetTransformToDevice(child, out transformToDevice);
        }

        CustomPopupPlacement preferredPlacement = CalculatePopupPlacement(placement, popupSize, targetSize, monitor, cursor, offset, target, child, transformToDevice);

        CustomPopupPlacement? alternativePlacement = null;
        var alternativePlacementMode = GetAlternativePlacementMode(placement);
        if (alternativePlacementMode.HasValue)
        {
            alternativePlacement = CalculatePopupPlacement(alternativePlacementMode.Value, popupSize, targetSize, monitor, cursor, offset, target, child, transformToDevice);
        }

        if (alternativePlacement.HasValue)
        {
            return [preferredPlacement, alternativePlacement.Value];
        }
        else
        {
            return [preferredPlacement];
        }
    }

    private static CustomPopupPlacement CalculatePopupPlacement(
        MenuFlyoutExPlacementMode placement,
        Size popupSize,
        Size targetSize,
        MonitorInfo monitor,
        Point? cursor,
        Point _,
        FrameworkElement target,
        FrameworkElement? child = null,
        Matrix transformToDevice = default)
    {
        Point point;
        PopupPrimaryAxis primaryAxis;

        switch (placement)
        {
            case MenuFlyoutExPlacementMode.AppBarTop:
                point = new Point((targetSize.Width - popupSize.Width) / 2, -popupSize.Height);
                primaryAxis = PopupPrimaryAxis.Horizontal;
                break;
            case MenuFlyoutExPlacementMode.AppBarBottom:
                point = new Point((targetSize.Width - popupSize.Width) / 2, targetSize.Height);
                primaryAxis = PopupPrimaryAxis.Horizontal;
                break;
            case MenuFlyoutExPlacementMode.AppBarLeft:
                point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) / 2);
                primaryAxis = PopupPrimaryAxis.Vertical;
                break;
            case MenuFlyoutExPlacementMode.AppBarRight:
                point = new Point(targetSize.Width, (targetSize.Height - popupSize.Height) / 2);
                primaryAxis = PopupPrimaryAxis.Vertical;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(placement));
        }

        if (cursor != null)
        {
            var cursorToScreenOffset = cursor.Value;
            if (transformToDevice != default)
            {
                cursorToScreenOffset = transformToDevice.Transform(cursorToScreenOffset);
            }
            var targetToScreenOffset = target.PointToScreen(new Point());
            targetToScreenOffset -= new Vector(monitor.Bounds.X, monitor.Bounds.Y);
            var cursorToTargetOffset = cursorToScreenOffset - targetToScreenOffset;
            switch (placement)
            {
                case MenuFlyoutExPlacementMode.AppBarTop:
                    point = new Point(cursorToTargetOffset.X - popupSize.Width / 2, point.Y);
                    break;
                case MenuFlyoutExPlacementMode.AppBarBottom:
                    point = new Point(cursorToTargetOffset.X - popupSize.Width / 2, point.Y);
                    break;
                case MenuFlyoutExPlacementMode.AppBarLeft:
                    point = new Point(point.X, cursorToTargetOffset.Y - popupSize.Height / 2);
                    break;
                case MenuFlyoutExPlacementMode.AppBarRight:
                    point = new Point(point.X, cursorToTargetOffset.Y - popupSize.Height / 2);
                    break;
            }
        }

        if (child != null) // Popup Presenter
        {
            var childToParentOffset = VisualTreeHelper.GetOffset(child);
            if (transformToDevice != default)
            {
                childToParentOffset = transformToDevice.Transform(childToParentOffset);
            }
            point -= childToParentOffset;
        }

        return new CustomPopupPlacement(point, primaryAxis);
    }

    private static MenuFlyoutExPlacementMode? GetAlternativePlacementMode(MenuFlyoutExPlacementMode placement)
    {
        return placement switch
        {
            MenuFlyoutExPlacementMode.AppBarTop => (MenuFlyoutExPlacementMode?)MenuFlyoutExPlacementMode.AppBarBottom,
            MenuFlyoutExPlacementMode.AppBarBottom => (MenuFlyoutExPlacementMode?)MenuFlyoutExPlacementMode.AppBarTop,
            MenuFlyoutExPlacementMode.AppBarLeft => (MenuFlyoutExPlacementMode?)MenuFlyoutExPlacementMode.AppBarRight,
            MenuFlyoutExPlacementMode.AppBarRight => (MenuFlyoutExPlacementMode?)MenuFlyoutExPlacementMode.AppBarLeft,
            _ => null,
        };
    }

    private static bool TryGetTransformToDevice(Visual visual, out Matrix value)
    {
        var presentationSource = PresentationSource.FromVisual(visual);
        if (presentationSource != null)
        {
            value = presentationSource.CompositionTarget.TransformToDevice;
            return true;
        }

        value = default;
        return false;
    }
}
