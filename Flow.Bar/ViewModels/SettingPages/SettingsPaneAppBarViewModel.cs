using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Controls.ContentDialogs;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Services;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel(AppBarManagementService appBarManagementService) : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarViewModel);

    public ObservableCollection<AppBarModel> AppBars { get; } = [];

    private readonly AppBarManagementService _appBarManagementService = appBarManagementService;

    [RelayCommand]
    private async Task AddAppBarAsync(Button button)
    {
        var dialog = new AddAppBarContentDialog() { Owner = Window.GetWindow(button) };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var model = new AppBarModel
            {
                DockMode = dialog.DockMode,
                MonitorName = dialog.MonitorName,
                FollowSystemTaskbarWidthOrHeight = dialog.FollowSystemTaskbarWidthOrHeight,
                DockedWidthOrHeight = dialog.DockedWidthOrHeight,
                IsResizable = dialog.IsResizable
            };
            _appBarManagementService.AddAppBar(model);
        }
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
