using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Services;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel(AppBarManagementService appBarManagementService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarViewModel);

    public ObservableCollection<AppBarModel> AppBars { get; } = [];

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    [RelayCommand]
    private void AddAppBar()
    {
        // TODO
    }

    private void RefreshAppBars()
    {
        AppBars.Clear();
        foreach (var appBar in _appBarManagementService.GetAllAppBars())
        {
            AppBars.Add(appBar);
        }
    }

    public void OnNavigatedTo(object? parameter)
    {
        RefreshAppBars();
    }

    public void OnNavigatedFrom()
    {

    }
}
