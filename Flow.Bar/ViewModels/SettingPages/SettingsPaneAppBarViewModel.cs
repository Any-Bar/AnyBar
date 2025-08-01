using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;

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
}
