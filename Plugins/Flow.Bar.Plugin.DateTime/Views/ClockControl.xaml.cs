using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Flow.Bar.Plugin.DateTime.Views;

public partial class DateTimeControl : UserControl
{
    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };

    public DateTimeControl(BarElementPosition position)
    {
        InitializeComponent();
        SetTime();
        switch (position)
        {
            case BarElementPosition.Left:
                TimeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TimeTextBlock.VerticalAlignment = VerticalAlignment.Center;
                DateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                DateTextBlock.VerticalAlignment = VerticalAlignment.Center;
                break;
            case BarElementPosition.Right:
                TimeTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                TimeTextBlock.VerticalAlignment = VerticalAlignment.Center;
                DateTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                DateTextBlock.VerticalAlignment = VerticalAlignment.Center;
                break;
            case BarElementPosition.Top:
                TimeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TimeTextBlock.VerticalAlignment = VerticalAlignment.Top;
                DateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                DateTextBlock.VerticalAlignment = VerticalAlignment.Top;
                break;
            case BarElementPosition.Bottom:
                TimeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TimeTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                DateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                DateTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case BarElementPosition.HorizontalCenter:
                TimeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TimeTextBlock.VerticalAlignment = VerticalAlignment.Center;
                DateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                DateTextBlock.VerticalAlignment = VerticalAlignment.Center;
                break;
            case BarElementPosition.VerticalCenter:
                TimeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TimeTextBlock.VerticalAlignment = VerticalAlignment.Top;
                DateTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                DateTextBlock.VerticalAlignment = VerticalAlignment.Top;
                break;
        }
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        SetTime();
    }

    private void SetTime()
    {
        TimeTextBlock.Text = System.DateTime.Now.ToShortTimeString();
        DateTextBlock.Text = System.DateTime.Now.ToShortDateString();
    }
}
