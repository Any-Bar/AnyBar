using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Flow.Bar.Plugin.Clock.Views;

public partial class ClockControl : UserControl
{
    private readonly DispatcherTimer _timer;

    public ClockControl()
    {
        InitializeComponent();
        TimeTextBlock.Text = DateTime.Now.ToShortTimeString();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        TimeTextBlock.Text = DateTime.Now.ToShortTimeString();
    }
}
