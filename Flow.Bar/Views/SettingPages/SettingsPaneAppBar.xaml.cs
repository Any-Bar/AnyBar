using System.Windows;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Models.AppBar;
using Flow.Bar.ViewModels.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Views.SettingPages;

public partial class SettingsPaneAppBar : Page
{
    private SettingsPaneAppBarViewModel _viewModel = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // If the navigation is not triggered by button click, view model will be null again
        if (_viewModel == null)
        {
            _viewModel = Ioc.Default.GetRequiredService<SettingsPaneAppBarViewModel>();
            DataContext = _viewModel;
        }
        if (!IsInitialized)
        {
            InitializeComponent();
            _viewModel.RefreshAppBars();
        }
        base.OnNavigatedTo(e);
    }

    private void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not SettingsCard element) return;
        if (element.Tag is not AppBarModel model) return;
        // TODO
    }
}
