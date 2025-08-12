using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Controls;

public class FontIconButton : Button
{
    static FontIconButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FontIconButton), new FrameworkPropertyMetadata(typeof(FontIconButton)));
    }

    public FontIconButton()
    {
    }
}
