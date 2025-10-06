using System;

namespace Flow.Bar.Plugin.Performance.Services;

public interface IPerformanceMonitorService
{
    event EventHandler<PerformanceDataEventArgs> PerformanceDataUpdated;

    void StartMonitoring();

    void StopMonitoring();

    PerformanceData GetCurrentPerformanceData();
}

public class PerformanceDataEventArgs(PerformanceData data) : EventArgs
{
    public PerformanceData Data { get; } = data;
}

public record PerformanceData(
    double CpuUsage,
    double MemoryUsage,
    double? GpuUsage
);
