using System;
using System.Timers;
using Windows.Win32;

namespace AnyBar.Models.Explorer;

public class FullScreenWindowWatcher
{
    private bool _fullScreenEntered = false;
    private readonly Timer _timer;

    public event Action<bool>? FullScreenEventInvoked;

    public FullScreenWindowWatcher()
    {
        _timer = new Timer(300); // check every 300 milliseconds
        _timer.Elapsed += (sender, args) =>
        {
            var fullScreenEntered = PInvokeHelper.IsForegroundWindowFullscreen();
            if (fullScreenEntered != _fullScreenEntered)
            {
                _fullScreenEntered = fullScreenEntered;
                FullScreenEventInvoked?.Invoke(fullScreenEntered);
            }
        };
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
