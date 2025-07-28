using System.Windows;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.DependencyInjection;
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
            RefreshAppBars();
        }
        base.OnNavigatedTo(e);
    }

    private void RefreshAppBars()
    {
        if (_viewModel.AppBars.Capacity == 0)
        {
            NoAppBarStackPanel.Visibility = Visibility.Visible;
            AppBarStackPanel.Visibility = Visibility.Collapsed;
            return;
        }

        NoAppBarStackPanel.Visibility = Visibility.Collapsed;
        AppBarStackPanel.Visibility = Visibility.Visible;
        AppBarStackPanel.Children.Clear();
        foreach (var appBar in _viewModel.AppBars)
        {
            var appBarControl = new SettingsExpander
            {
                Header = "Appbar",
                Content = new System.Windows.Controls.CheckBox()
            };
            AppBarStackPanel.Children.Add(appBarControl);
        }
    }
}
