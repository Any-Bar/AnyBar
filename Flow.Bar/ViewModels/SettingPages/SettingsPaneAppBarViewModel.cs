using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Bar.Models.AppBar;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAppBarViewModel : ObservableObject
{
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
}
