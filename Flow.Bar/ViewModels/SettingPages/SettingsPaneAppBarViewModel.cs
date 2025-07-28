using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.UserSettings;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel(Settings settings) : ObservableObject
{
    private readonly Settings _settings = settings;

    public List<AppBarModel> AppBars => [.. _settings.AppBars.Values.OrderBy(bar => bar.Order)];

    [RelayCommand]
    private void AddAppBar()
    {

    }
}
