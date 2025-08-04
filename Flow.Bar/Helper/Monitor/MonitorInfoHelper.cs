using Flow.Bar.Models.Monitor;
using System.Linq;

namespace Flow.Bar.Helper.Monitor;

public class MonitorInfoHelper
{
    public static MonitorInfo? GetMonitorInfoFromName(string? monitorName)
    {
        if (monitorName == null)
        {
            var primaryMonitor = MonitorInfo.GetPrimaryDisplayMonitor();
            if (primaryMonitor != null)
            {
                return primaryMonitor;
            }
            else
            {
                var allMonitors = MonitorInfo.GetDisplayMonitors();
                return allMonitors.Length > 0 ? allMonitors[0] : null;
            }
        }
        else
        {
            var allMonitors = MonitorInfo.GetDisplayMonitors();
            return allMonitors.FirstOrDefault(m => m.Name == monitorName)
                ?? allMonitors.FirstOrDefault(m => m.IsPrimary)
                ?? (allMonitors.Length > 0 ? allMonitors[0] : null);
        }
    }
}
