using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Controls;
using Flow.Bar.Models.Plugins;
using Flow.Bar.ViewModels.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Navigation;
using MenuItem = System.Windows.Controls.MenuItem;
using HeaderedItemsControl = System.Windows.Controls.HeaderedItemsControl;

namespace Flow.Bar.Views.SettingPages;

public partial class SettingsPanePlugins : Page
{
    private const double ContextMenuWidth = 275;
    private const double UninstallConfirmationContextMenuWidth = 300;
    private const double UninstallConfirmationContextMenuHeight = 118;

    private SettingsPanePluginsViewModel _viewModel = null!;

    private MenuFlyoutEx _contextMenu = null!;
    private FontIconButton? _button = null;
    private PluginViewModel? _contextMenuPlugin = null;
    private bool _openUninstallConfirmationContextMenu = false;

    private MenuFlyoutEx _uninstallConfirmationContextMenu = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // If the navigation is not triggered by button click, view model will be null again
        if (_viewModel == null)
        {
            _viewModel = Ioc.Default.GetRequiredService<SettingsPanePluginsViewModel>();
            DataContext = _viewModel;
        }
        if (_contextMenu == null)
        {
            _contextMenu = new()
            {
                Width = ContextMenuWidth
            };
            var settingItem = new MenuItem();
            settingItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPanePlugins_Uninstall));
            settingItem.Click += (o, e) =>
            {
                if (_contextMenuPlugin != null && _button != null)
                {
                    _openUninstallConfirmationContextMenu = true;
                }
            };
            _contextMenu.Items.Add(settingItem);
            _contextMenu.Closed += ContextMenu_Closed;
        }
        if (_uninstallConfirmationContextMenu == null)
        {
            _uninstallConfirmationContextMenu = new()
            {
                Width = UninstallConfirmationContextMenuWidth,
                Height = UninstallConfirmationContextMenuHeight,
                MenuFlyoutPresenterStyle = (Style)Application.Current.Resources["UninstallConfirmationContextMenuStyle"]
            };
        }
        if (!IsInitialized)
        {
            InitializeComponent();
        }
        base.OnNavigatedTo(e);
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        if (_openUninstallConfirmationContextMenu)
        {
            _openUninstallConfirmationContextMenu = false;
            if (_contextMenuPlugin != null && _button != null)
            {
                _uninstallConfirmationContextMenu.ShowAt(_button, new MenuFlyoutExOptions()
                {
                    Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight
                });
            }
        }
        _contextMenuPlugin = null;
        _button = null;
    }

    private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FontIconButton button) return;
        if (button.Tag is not PluginViewModel plugin) return;
        _button = button;
        _contextMenuPlugin = plugin;
        _contextMenu.ShowAt(button, new MenuFlyoutExOptions()
        {
            Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight
        });
    }
}
