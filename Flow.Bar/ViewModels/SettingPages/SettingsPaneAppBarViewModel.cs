using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models;
using System;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel : ObservableObject
{
    public List<AppBarModel> AppBars { get; } = [.. App.Settings.AppBars.Values.OrderBy(bar => bar.Order)];

    public List<AppBarDockModeLocalized> AllDockModes { get; } = AppBarDockModeLocalized.GetValues();

    public List<MonitorNameLocalized> AllMonitors { get; } = MonitorNameLocalized.GetValues(App.Settings.AppBars.Values);

    [RelayCommand]
    private void AddAppBar()
    {

    }

    public class MonitorNameLocalized
    {
        public string? Value { get; private init; }
        public required string Display { get; set; }
        public string? LocalizationKey { get; set; }
        public string? LocalizationValue { get; set; }

        public static List<MonitorNameLocalized> GetValues(ICollection<AppBarModel> appBars)
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
            // Sort the monitors by name
            monitorList.Sort((x, y) => string.Compare(x.Display, y.Display, StringComparison.OrdinalIgnoreCase));
            // Add a primary monitor option at the top
            monitorList.Insert(0, new MonitorNameLocalized
            {
                Value = null,
                Display = Localize.SettingPaneAppBar_MonitorPrimary(),
                LocalizationKey = nameof(Localize.SettingPaneAppBar_MonitorPrimary)
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
}
