using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Helper.MenuFlyout;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.Plugins;
using Flow.Bar.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Navigation;
using HeaderedItemsControl = System.Windows.Controls.HeaderedItemsControl;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Flow.Bar.Views;

public partial class SettingsPanePlugins : Page
{
    private static readonly double ContextMenuWidth = (double)Application.Current.TryFindResource("CustomContextMenuWidth");
    private static readonly double SecondaryContextMenuWidth = (double)Application.Current.TryFindResource("SecondaryContextMenuWidth");
    private static readonly double SecondaryContextMenuHeight = (double)Application.Current.TryFindResource("SecondaryContextMenuHeight");
    private static readonly Style PluginUninstallationContextMenuStyle = (Style)Application.Current.TryFindResource("PluginUninstallationContextMenuStyle");

    private SettingsPanePluginsViewModel _viewModel = null!;

    private PluginUninstallationMenuFlyoutHelper<PluginViewModel> _menuFlyoutHelper = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // If the navigation is not triggered by button click, view model will be null again
        if (_viewModel == null)
        {
            _viewModel = Ioc.Default.GetRequiredService<SettingsPanePluginsViewModel>();
            DataContext = _viewModel;
        }
        if (_menuFlyoutHelper == null)
        {
            _menuFlyoutHelper = new(
                ContextMenuWidth,
                SecondaryContextMenuWidth,
                SecondaryContextMenuHeight,
                PluginUninstallationContextMenuStyle,
                "UninstallButton",
                UninstallPlugin);
            var uninstallItem = new MenuItem();
            uninstallItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPanePlugins_Uninstall));
            uninstallItem.Click += UninstallItem_Click;
            _menuFlyoutHelper.Items.Add(uninstallItem);
        }
        if (!IsInitialized)
        {
            InitializeComponent();
        }
        base.OnNavigatedTo(e);
    }

    private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        _menuFlyoutHelper.ButtonClick(sender, e);
    }

    private void UninstallItem_Click(object sender, RoutedEventArgs e)
    {
        _menuFlyoutHelper.UninstallItemClick(sender, e);
    }

    private async void UninstallPlugin(PluginViewModel plugin)
    {
        var oldPlugin = plugin.PluginPair.Metadata;
        if (await PluginInstaller.UninstallPluginAndCheckRestartAsync(oldPlugin))
        {
            _viewModel.UninstallPlugin(oldPlugin);
        }
    }
}
