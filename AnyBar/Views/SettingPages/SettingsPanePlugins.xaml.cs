using System.Windows.Navigation;
using CommunityToolkit.Mvvm.DependencyInjection;
using AnyBar.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;

namespace AnyBar.Views;

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
