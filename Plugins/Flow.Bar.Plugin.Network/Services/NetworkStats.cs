// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Flow.Bar.Plugin.Network.Services;

public sealed class NetworkStats : IDisposable
{
    private readonly Dictionary<string, List<PerformanceCounter>> _networkCounters = [];

    private Dictionary<string, Data> NetworkUsages { get; set; } = [];

    private Dictionary<string, List<float>> NetChartValues { get; set; } = [];

    private sealed class Data
    {
        public float Usage { get; set; }

        public float Sent { get; set; }

        public float Received { get; set; }
    }

    public void InitNetworkPerfCounters()
    {
        _networkCounters.Clear();
        var pcc = new PerformanceCounterCategory("Network Interface");
        var instanceNames = pcc.GetInstanceNames();
        foreach (var instanceName in instanceNames)
        {
            var instanceCounters = new List<PerformanceCounter>
            {
                new("Network Interface", "Bytes Sent/sec", instanceName),
                new("Network Interface", "Bytes Received/sec", instanceName),
                new("Network Interface", "Current Bandwidth", instanceName)
            };
            _networkCounters.Add(instanceName, instanceCounters);
            NetChartValues.Add(instanceName, []);
            NetworkUsages.Add(instanceName, new Data());
        }
    }

    public (float TotalSent, float TotalReceived, float MaxUsage) GetData()
    {
        float totalSent = 0;
        float totalReceived = 0;
        float maxUsage = 0;
        foreach (var networkCounterWithName in _networkCounters)
        {
            try
            {
                var sent = networkCounterWithName.Value[0].NextValue();
                var received = networkCounterWithName.Value[1].NextValue();
                var bandWidth = networkCounterWithName.Value[2].NextValue();
                if (bandWidth == 0)
                {
                    continue;
                }

                var usage = 8 * (sent + received) / bandWidth;
                var name = networkCounterWithName.Key;
                NetworkUsages[name].Sent = sent;
                NetworkUsages[name].Received = received;
                NetworkUsages[name].Usage = usage;

                var chartValues = NetChartValues[name];

                totalSent += sent;
                totalReceived += received;
                if (usage > maxUsage)
                {
                    maxUsage = usage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting network data: {ex.Message}");
            }
        }

        return (totalSent, totalReceived, maxUsage);
    }

    public void Dispose()
    {
        foreach (var counterPair in _networkCounters)
        {
            foreach (var counter in counterPair.Value)
            {
                counter.Dispose();
            }
        }
    }
}
