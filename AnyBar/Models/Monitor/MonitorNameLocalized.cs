using System;
using System.Collections.Generic;
using System.Linq;
using AnyBar.Enums;
using AnyBar.Models.AppBar;

namespace AnyBar.Models.Monitor;

public class MonitorNameLocalized
{
    public string? Value { get; private init; }
    public required string Display { get; set; }
    public string? LocalizationKey { get; set; }
    public string? LocalizationValue { get; set; }

    public static List<MonitorNameLocalized> GetValues(ICollection<AppBarModel>? appBars)
    {
        // Get all monitors from the system
        var monitorList = MonitorInfo.GetDisplayMonitors()
            .Select(monitor => new MonitorNameLocalized
            {
                Value = monitor.Name,
                Display = monitor.Name,
                LocalizationValue = monitor.Name
            })
            .ToList();
        // Include monitors from app bars if provided
        if (appBars is not null)
        {
            var monitorNames = monitorList.Select(m => m.Value).ToHashSet();
            // Append monitors that are not included in the system
            foreach (var appBar in appBars)
            {
                var monitorName = appBar.MonitorName;
                if (monitorName is not null && !monitorNames.Contains(monitorName))
                {
                    monitorList.Add(new MonitorNameLocalized
                    {
                        Value = monitorName,
                        Display = monitorName,
                        LocalizationValue = monitorName
                    });
                    monitorNames.Add(monitorName);
                }
            }
        }
        // Sort the monitors by name
        monitorList.Sort((x, y) => string.Compare(x.Display, y.Display, StringComparison.OrdinalIgnoreCase));
        // Add a primary monitor option at the top
        monitorList.Insert(0, new MonitorNameLocalized
        {
            Value = null,
            Display = Localize.SettingPaneAppBarSetting_MonitorPrimary(),
            LocalizationKey = nameof(Localize.SettingPaneAppBarSetting_MonitorPrimary)
        });
        return monitorList;
    }

    public static void UpdateLabels(List<AppBarDockModeLocalized> options)
    {
        foreach (var item in options)
        {
            if (!string.IsNullOrWhiteSpace(item.LocalizationKey))
            {
                item.Display = PublicApi.Instance.GetTranslation(item.LocalizationKey);
            }
        }
    }
}
