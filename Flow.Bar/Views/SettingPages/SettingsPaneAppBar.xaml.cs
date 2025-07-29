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
                Header = $"{Localize.SettingWindow_AppBar()} {appBar.Order + 1}",
                HeaderIcon = new FontIcon { Glyph = "\uE90E" },
                Tag = appBar,
                Content = new ToggleSwitch()
            };
            var dockModeCard = new SettingsCard
            {
                Header = Localize.SettingPaneAppBar_DockMode(),
                Content = new System.Windows.Controls.ComboBox()
            };
            var monitorCard = new SettingsCard
            {
                Header = Localize.SettingPaneAppBar_Monitor(),
                Content = new System.Windows.Controls.ComboBox()
            };
            var resizableCard = new SettingsCard
            {
                Header = Localize.SettingPaneAppBar_Resizable(),
                Content = new ToggleSwitch()
            };
            var elementsCard = new SettingsCard
            {
                Header = Localize.SettingPaneAppBar_BarElements(),
                Content = new System.Windows.Controls.Button { Content = Localize.SettingPaneAppBar_Edit()}
            };
            appBarControl.Items.Add(dockModeCard);
            appBarControl.Items.Add(monitorCard);
            appBarControl.Items.Add(resizableCard);
            appBarControl.Items.Add(elementsCard);
            AppBarStackPanel.Children.Add(appBarControl);
        }
    }
}
