using CommunityToolkit.Mvvm.DependencyInjection;
using Flow.Bar.Controls;
using Flow.Bar.Helper.Plugins;
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
    private static readonly double UninstallConfirmationContextMenuWidth = (double)Application.Current.TryFindResource("UninstallConfirmationContextMenuWidth");
    private static readonly double UninstallConfirmationContextMenuHeight = (double)Application.Current.TryFindResource("UninstallConfirmationContextMenuHeight");

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
            var uninstallItem = new MenuItem();
            uninstallItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPanePlugins_Uninstall));
            uninstallItem.Click += UninstallItem_Click;
            _contextMenu.Items.Add(uninstallItem);
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
            _uninstallConfirmationContextMenu.ButtonClickEvents.Add("UninstallButton", UninstallButton_Click);
            _uninstallConfirmationContextMenu.Closed += UninstallConfirmationContextMenu_Closed;
        }
        if (!IsInitialized)
        {
            InitializeComponent();
        }
        base.OnNavigatedTo(e);
    }

    private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        _contextMenuPlugin = null;
        _button = null;
        if (sender is not FontIconButton button) return;
        if (button.Tag is not PluginViewModel plugin) return;
        _button = button;
        _contextMenuPlugin = plugin;
        _contextMenu.ShowAt(button, new MenuFlyoutExOptions()
        {
            Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight
        });
    }

    private void UninstallItem_Click(object sender, RoutedEventArgs e)
    {
        if (_contextMenuPlugin != null && _button != null)
        {
            _openUninstallConfirmationContextMenu = true;
        }
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
        else
        {
            _contextMenuPlugin = null;
            _button = null;
        }
    }

    private async void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        var oldPlugin = _contextMenuPlugin?.PluginPair.Metadata;
        _uninstallConfirmationContextMenu.Hide();
        if (oldPlugin != null && await PluginInstaller.UninstallPluginAndCheckRestartAsync(oldPlugin))
        {
            _viewModel.UninstallPlugin(oldPlugin);
        }
    }

    private void UninstallConfirmationContextMenu_Closed(object? sender, object? e)
    {
        _openUninstallConfirmationContextMenu = false;
        _contextMenuPlugin = null;
        _button = null;
    }
}
