using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Interfaces;
using Flow.Bar.Models.AppBar;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel : ObservableObject, INavigationAware
{
    private static readonly string ClassName = nameof(SettingsPaneAppBarViewModel);

    public ObservableCollection<AppBarModel> AppBars { get; } = [];

    [RelayCommand]
    private void AddAppBar()
    {
        // TODO
    }

    internal void RefreshAppBars()
    {
        AppBars.Clear();
        foreach (var appBar in App.Settings.AppBars.Values.OrderBy(bar => bar.Order))
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
