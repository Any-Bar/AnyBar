using System;
using System.Threading;

namespace Flow.Bar.Plugin.Network.Services;

#nullable enable

public class NetworkMonitorService : INetworkMonitorService, IDisposable
{
    private readonly Timer _timer;
    private readonly Lock _lockObject = new();
    private readonly NetworkStats _networkStats = new();
    private bool _isMonitoring;

    public event EventHandler<NetworkDataEventArgs>? NetworkDataUpdated;

    public NetworkMonitorService()
    {
        _timer = new Timer(OnTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        _networkStats.InitNetworkPerfCounters();
    }

    public void Dispose()
    {
        StopMonitoring();
        _timer?.Dispose();
        _networkStats?.Dispose();
    }

    public void StartMonitoring()
    {
        lock (_lockObject)
        {
            if (!_isMonitoring)
            {
                _isMonitoring = true;
                _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }
        }
    }

    public void StopMonitoring()
    {
        lock (_lockObject)
        {
            if (_isMonitoring)
            {
                _isMonitoring = false;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }

    public NetworkData GetCurrentNetworkData()
    {
        var (uploadSpeed, downloadSpeed, maxUsage) = _networkStats.GetData();

        return new NetworkData(
            uploadSpeed,
            downloadSpeed,
            maxUsage
        );
    }

    private void OnTimerCallback(object? state)
    {
        if (!_isMonitoring)
        {
            return;
        }

        try
        {
            NetworkData performanceData = GetCurrentNetworkData();
            NetworkDataUpdated?.Invoke(this, new NetworkDataEventArgs(performanceData));
        }
        catch (Exception ex)
        {
            // Log error but continue monitoring
            System.Diagnostics.Debug.WriteLine($"Error getting performance data: {ex.Message}");
        }
    }
}
