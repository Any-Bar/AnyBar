using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace AnyBar.Models.Explorer;

public class ExplorerWatcher : IDisposable
{
    private int _lastExplorerProcessId = 0;
    private readonly Timer _timer;

    public event Action? ExplorerRestarted;

    public ExplorerWatcher()
    {
        UpdateExplorerProcessId();
        _timer = new Timer(300); // check every 300 milliseconds
        _timer.Elapsed += (sender, args) =>
        {
            if (!IsExplorerRunning(_lastExplorerProcessId))
            {
                UpdateExplorerProcessId();
                ExplorerRestarted?.Invoke();
            }
        };
    }

    public void Start()
    {
        _timer.Start();
    }

    private static bool IsExplorerRunning(int pid)
    {
        try
        {
            var proc = Process.GetProcessById(pid);
            return !proc.HasExited && proc.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private void UpdateExplorerProcessId()
    {
        var explorer = Process.GetProcessesByName("explorer").OrderByDescending(p => p.StartTime).FirstOrDefault();
        _lastExplorerProcessId = explorer?.Id ?? 0;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
