using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows.Navigation;

namespace Flow.Bar.Views;

public partial class SettingsPanePlugins : Page
{
    private SettingsPanePluginsViewModel _viewModel = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // If the navigation is not triggered by button click, view model will be null again
        if (_viewModel == null)
        {
            _viewModel = Ioc.Default.GetRequiredService<SettingsPanePluginsViewModel>();
            DataContext = _viewModel;
        }
        if (!IsInitialized)
        {
            InitializeComponent();
        }
        base.OnNavigatedTo(e);
    }
}
