using System.Media;
using System.Windows;
using System.Windows.Media;

namespace Flow.Bar.Controls;

public partial class MessageBoxEx
{
    public static MessageBoxResult Show(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, ImageSource? icon = null, MessageBoxResult defaultResult = MessageBoxResult.OK, SystemSound? sound = null)
    {
        var window = new MessageBoxEx
        {
            Owner = null,
            ImageSource = icon,
            _result = defaultResult,
            Content = messageBoxText,
            MessageBoxButtons = button,
            Caption = caption ?? string.Empty,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        if (MakeSound)
        {
            window.SystemSoundOnLoaded = sound;
        }

        return window.ShowDialog();
    }
}
