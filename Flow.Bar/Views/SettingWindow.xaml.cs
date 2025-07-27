using Flow.Bar.Views.SettingPages;
using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace Flow.Bar.Views;

public partial class SettingWindow : Window
{
    private SettingPageTag? _lastItem = null;

    public SettingWindow()
    {
        InitializeComponent();
    }

    #region Window Events

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0]; // Select the first item by default
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        App.API.SaveAppAllSettings();
    }

    #endregion

    #region Navigation View Events

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        Thickness currMargin = AppTitleBar.Margin;
        if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            AppTitleBar.Margin = new Thickness((sender.CompactPaneLength * 2), currMargin.Top, currMargin.Right, currMargin.Bottom);

        }
        else
        {
            AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }
        AppTitleBar.Visibility = sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top ? Visibility.Collapsed : Visibility.Visible;
        UpdateAppTitleMargin(sender);
    }

    private void UpdateAppTitleMargin(NavigationView _)
    {
        const int smallLeftIndent = 2;

        Thickness currMargin = AppTitle.Margin;
        AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
    }

    private void NavigationViewControl_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        UpdateAppTitleMargin(sender);
    }

    private void NavigationViewControl_PaneOpening(NavigationView sender, object args)
    {
        UpdateAppTitleMargin(sender);
    }

    private void NavigationViewControl_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            throw new NotImplementedException("Settings page is not implemented yet.");
        }
        else
        {
            var selectedItem = args.SelectedItemContainer;

            if (selectedItem?.Tag is SettingPageTag tag)
            {
                var item = tag;
                if (item == _lastItem) return;

                _lastItem = item;
                RootFrame.Navigate(GetPageType(item));
            }
            else
            {
                throw new InvalidOperationException("Selected item does not have a valid tag.");
            }
        }
    }

    private static Type GetPageType(SettingPageTag tag)
    {
        return tag switch
        {
            SettingPageTag.AppBar => typeof(SettingsPaneAppBar),
            SettingPageTag.About => typeof(SettingsPaneAbout),
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
    }

    private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
    {

    }

    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // Update the selected NavigationViewItem based on the page type
        NavigationViewItem? newItem = null;

        if (RootFrame.SourcePageType == typeof(SettingsPaneAppBar))
        {
            _lastItem = SettingPageTag.AppBar;
            newItem = AppBarItem;
        }
        else if (RootFrame.SourcePageType == typeof(SettingsPaneAbout))
        {
            _lastItem = SettingPageTag.About;
            newItem = AboutItem;
        }
        else
        {
            throw new InvalidOperationException("RootFrame is not navigated to a valid page.");
        }

        if (newItem != null && NavigationViewControl.SelectedItem != newItem)
        {
            NavigationViewControl.SelectedItem = newItem;
        }
    }

    #endregion
}

public enum SettingPageTag
{
    AppBar,
    About
}
