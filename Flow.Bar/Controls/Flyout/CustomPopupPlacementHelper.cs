using Flow.Bar.Models;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Flow.Bar.Controls.Flyout
{
    internal static class CustomPopupPlacementHelper
    {
        #region Placement

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.RegisterAttached(
                "Placement",
                typeof(AppBarPlacementMode),
                typeof(CustomPopupPlacementHelper),
                new PropertyMetadata(AppBarPlacementMode.Top));

        public static AppBarPlacementMode GetPlacement(DependencyObject element)
        {
            return (AppBarPlacementMode)element.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject element, AppBarPlacementMode value)
        {
            element.SetValue(PlacementProperty, value);
        }

        #endregion

        internal static CustomPopupPlacement[] PositionPopup(
            AppBarPlacementMode placement,
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
            AppBarPlacementMode placement,
            Size popupSize,
            Size targetSize,
            MonitorInfo monitor,
            Point? cursor,
            Point offset,
            FrameworkElement target,
            FrameworkElement? child = null,
            Matrix transformToDevice = default)
        {
            Point point;
            PopupPrimaryAxis primaryAxis;

            switch (placement)
            {
                case AppBarPlacementMode.Top:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, -popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case AppBarPlacementMode.Bottom:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, targetSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case AppBarPlacementMode.Left:
                    point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case AppBarPlacementMode.Right:
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
                    case AppBarPlacementMode.Top:
                        point = new Point(cursorToTargetOffset.X - popupSize.Width / 2, point.Y);
                        break;
                    case AppBarPlacementMode.Bottom:
                        point = new Point(cursorToTargetOffset.X - popupSize.Width / 2, point.Y);
                        break;
                    case AppBarPlacementMode.Left:
                        point = new Point(point.X, cursorToTargetOffset.Y - popupSize.Height / 2);
                        break;
                    case AppBarPlacementMode.Right:
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

        private static AppBarPlacementMode? GetAlternativePlacementMode(AppBarPlacementMode placement)
        {
            return placement switch
            {
                AppBarPlacementMode.Top => (AppBarPlacementMode?)AppBarPlacementMode.Bottom,
                AppBarPlacementMode.Bottom => (AppBarPlacementMode?)AppBarPlacementMode.Top,
                AppBarPlacementMode.Left => (AppBarPlacementMode?)AppBarPlacementMode.Right,
                AppBarPlacementMode.Right => (AppBarPlacementMode?)AppBarPlacementMode.Left,
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
}
