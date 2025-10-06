using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Flow.Bar.Plugin.DateTime.Views;

public partial class DateTimeControl : UserControl
{
    private static readonly string ClassName = nameof(DateTimeControl);

    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };

    public DateTimeControl(BarElementPosition position)
    {
        InitializeComponent();
        OnDockModeChanged(position);
        SetTime();
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

    public void OnDockModeChanged(BarElementPosition position)
    {
        Main.Context.API.LogVerbose(ClassName, $"Changed dock mode to {position}");
        switch (position)
        {
            case BarElementPosition.Left:
            case BarElementPosition.HorizontalCenter:
                SetElementAlignment(HorizontalAlignment.Left, VerticalAlignment.Center);
                break;
            case BarElementPosition.Right:
                SetElementAlignment(HorizontalAlignment.Right, VerticalAlignment.Center);
                break;
            case BarElementPosition.Top:
            case BarElementPosition.VerticalCenter:
                SetElementAlignment(HorizontalAlignment.Left, VerticalAlignment.Top);
                break;
            case BarElementPosition.Bottom:
                SetElementAlignment(HorizontalAlignment.Left, VerticalAlignment.Bottom);
                break;
        }
    }

    private void SetElementAlignment(HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        TimeTextBlock.HorizontalAlignment = horizontal;
        TimeTextBlock.VerticalAlignment = vertical;
        DateTextBlock.HorizontalAlignment = horizontal;
        DateTextBlock.VerticalAlignment = vertical;
    }
}
