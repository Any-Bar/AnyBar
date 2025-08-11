using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Helper.MenuFlyout;
using Flow.Bar.Models.AppBar;
using Flow.Bar.Services;
using Flow.Bar.ViewModels.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Navigation;
using HeaderedItemsControl = System.Windows.Controls.HeaderedItemsControl;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Flow.Bar.Views.SettingPages;

public partial class SettingsPaneBarElementSetting : Page
{
    private static readonly double ContextMenuWidth = (double)Application.Current.TryFindResource("CustomContextMenuWidth");
    private static readonly double SecondaryContextMenuWidth = (double)Application.Current.TryFindResource("SecondaryContextMenuWidth");
    private static readonly double SecondaryContextMenuHeight = (double)Application.Current.TryFindResource("SecondaryContextMenuHeight");
    private static readonly Style BarElementRemoveContextMenuStyle = (Style)Application.Current.TryFindResource("BarElementRemoveContextMenuStyle");

    private AppBarManagementService _appBarManagementService = null!;

    private SettingsPaneBarElementSettingViewModel _viewModel = null!;

    private PluginUninstallationMenuFlyoutHelper<BarElementViewModel> _menuFlyoutHelper = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // If the navigation is not triggered by button click, view model will be null again
        if (_viewModel == null)
        {
            _appBarManagementService = Ioc.Default.GetRequiredService<AppBarManagementService>();
            _viewModel = Ioc.Default.GetRequiredService<SettingsPaneBarElementSettingViewModel>();
            DataContext = _viewModel;
        }
        if (_menuFlyoutHelper == null)
        {
            _menuFlyoutHelper = new(
                ContextMenuWidth,
                SecondaryContextMenuWidth,
                SecondaryContextMenuHeight,
                BarElementRemoveContextMenuStyle,
                "RemoveButton",
                RemoveBarElement);
            var uninstallItem = new MenuItem();
            uninstallItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPaneAppBarSetting_Remove));
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

    private void RemoveBarElement(BarElementViewModel plugin)
    {
        _viewModel.UninstallBarElement(plugin);
    }
}
