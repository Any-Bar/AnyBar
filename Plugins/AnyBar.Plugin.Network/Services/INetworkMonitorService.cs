using System;

namespace AnyBar.Plugin.Network.Services;

public interface INetworkMonitorService
{
    event EventHandler<NetworkDataEventArgs> NetworkDataUpdated;

    void StartMonitoring();

    void StopMonitoring();

    NetworkData GetCurrentNetworkData();
}

public class NetworkDataEventArgs(NetworkData data) : EventArgs
{
    public NetworkData Data { get; } = data;
}

public record NetworkData(
    float UploadSpeed,
    float DownloadSpeed,
    float MaxUsage
);
