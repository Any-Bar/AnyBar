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

    private SettingsPanePluginsViewModel _viewModel = null!;

    private MenuFlyoutEx _contextMenu = null!;
    private PluginViewModel? _contextMenuPlugin = null;

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
            _contextMenu = new();
            var settingItem = new MenuItem()
            {
                Width = ContextMenuWidth,
            };
            settingItem.SetResourceReference(HeaderedItemsControl.HeaderProperty, nameof(Localize.SettingPanePlugins_Uninstall));
            settingItem.Click += (o, e) =>
            {
                if (_contextMenuPlugin != null)
                {
                    // TODO
                }
            };
            _contextMenu.Items.Add(settingItem);
            _contextMenu.Closed += ContextMenu_Closed;
        }
        if (!IsInitialized)
        {
            InitializeComponent();
        }
        base.OnNavigatedTo(e);
    }

    private void ContextMenu_Closed(object? sender, object? e)
    {
        _contextMenuPlugin = null;
    }

    private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FontIconButton button) return;
        if (button.Tag is not PluginViewModel plugin) return;
        _contextMenuPlugin = plugin;
        _contextMenu.ShowAt(button, new MenuFlyoutExOptions()
        {
            Placement = MenuFlyoutExPlacementMode.BottomEdgeAlignedRight
        });
    }
}
