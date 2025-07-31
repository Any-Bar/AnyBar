using System.Windows;
using System.Windows.Input;

namespace Flow.Bar.Controls.NavigationView;

internal interface IGettingFocusEventArgs2
{
    bool TrySetNewFocusedElement(DependencyObject element);
}

internal class GettingFocusEventArgs : IGettingFocusEventArgs2
{
    internal GettingFocusEventArgs(KeyboardFocusChangedEventArgs args)
    {
        InputDevice = InputManager.Current.MostRecentInputDevice switch
        {
            MouseDevice _ => FocusInputDeviceKind.Mouse,
            TouchDevice _ => FocusInputDeviceKind.Touch,
            StylusDevice _ => FocusInputDeviceKind.Pen,
            TabletDevice _ => FocusInputDeviceKind.Pen,
            KeyboardDevice _ => FocusInputDeviceKind.Keyboard,
            _ => FocusInputDeviceKind.Mouse
        };

        OldFocusedElement = args.OldFocus as DependencyObject;
        NewFocusedElement = args.NewFocus as DependencyObject;
    }

    public bool TrySetNewFocusedElement(DependencyObject element)
    {
        if (element is IInputElement inputElement && Keyboard.Focus(inputElement) == inputElement)
        {
            Cancel = true;
            return true;
        }

        return false;
    }

    public DependencyObject? NewFocusedElement { get; set; }
    public bool Handled { get; set; }
    public bool Cancel { get; set; }
    public FocusInputDeviceKind InputDevice { get; }
    public DependencyObject? OldFocusedElement { get; }
}
