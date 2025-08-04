using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Controls;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Models.Enums;
using Flow.Bar.Services;
using Flow.Bar.ViewModels.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Navigation;

namespace Flow.Bar.Views.SettingPages;

public partial class SettingsPaneAppBar : Page
{
    private SettingsPaneAppBarViewModel _viewModel = null!;
    private NavigationViewService _navigationService = null!;
    private AppBarManagementService _appBarManagementService = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // If the navigation is not triggered by button click, view model will be null again
        if (_viewModel == null)
        {
            _viewModel = Ioc.Default.GetRequiredService<SettingsPaneAppBarViewModel>();
            _navigationService = Ioc.Default.GetRequiredService<NavigationViewService>();
            _appBarManagementService = Ioc.Default.GetRequiredService<AppBarManagementService>();
            DataContext = _viewModel;
        }
        if (!IsInitialized)
        {
            InitializeComponent();
        }
        base.OnNavigatedTo(e);
    }

    private void AppBarSettingsCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not SettingsCard element) return;
        if (element.Tag is not AppBarModel model) return;
        _navigationService.NavigateTo(SettingPageTag.AppBarSetting, model);
    }

    private void AppBarToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitchEx toggleSwitch) return;
        if (toggleSwitch.Tag is not AppBarModel model) return;
        _appBarManagementService.SetEnabled(model.Order, toggleSwitch.IsOn);
    }
}
