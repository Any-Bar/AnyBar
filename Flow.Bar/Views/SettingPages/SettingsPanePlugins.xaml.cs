using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Helper.MenuFlyout;
using Flow.Bar.Helper.Plugins;
using Flow.Bar.Models.Plugins;
using Flow.Bar.ViewModels.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Navigation;
using HeaderedItemsControl = System.Windows.Controls.HeaderedItemsControl;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Flow.Bar.Views.SettingPages;

public partial class SettingsPanePlugins : Page
{
    private static readonly double ContextMenuWidth = (double)Application.Current.TryFindResource("CustomContextMenuWidth");
    private static readonly double UninstallConfirmationContextMenuWidth = (double)Application.Current.TryFindResource("UninstallConfirmationContextMenuWidth");
    private static readonly double UninstallConfirmationContextMenuHeight = (double)Application.Current.TryFindResource("UninstallConfirmationContextMenuHeight");
    private static readonly Style UninstallConfirmationContextMenuStyle = (Style)Application.Current.TryFindResource("UninstallConfirmationContextMenuStyle");

    private SettingsPanePluginsViewModel _viewModel = null!;

    private PluginUninstallationMenuFlyoutHelper _menuFlyoutHelper = null!;

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
                UninstallConfirmationContextMenuWidth,
                UninstallConfirmationContextMenuHeight,
                UninstallConfirmationContextMenuStyle,
                "UninstallButton",
                UninstallButton_Click);
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

    private async void UninstallButton_Click(PluginViewModel plugin)
    {
        var oldPlugin = plugin.PluginPair.Metadata;
        if (await PluginInstaller.UninstallPluginAndCheckRestartAsync(oldPlugin))
        {
            _viewModel.UninstallPlugin(oldPlugin);
        }
    }
}
