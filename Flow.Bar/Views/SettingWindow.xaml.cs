using Flow.Bar.Controls.NavigationView;
using Flow.Bar.Views.SettingPages;
using iNKORE.UI.WPF.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Flow.Bar.Views;

public partial class SettingWindow : Window
{
    private SettingPageTag? _lastItem = null;

    private TextBlock? _pageHeader = null;
    public TextBlock? PageHeader
    {
        get
        {
            return _pageHeader ??= VisualTree.FindDescendants<TextBlock>(NavigationViewControl)
                .FirstOrDefault(tb => tb.Name == "TitleTextBlock");
        }
    }

    public SettingWindow()
    {
        InitializeComponent();
    }

    #region Window Events

    private void Window_Closed(object sender, EventArgs e)
    {
        App.API.SaveAppAllSettings();
    }

    #endregion

    #region Navigation View Events

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0]; // Select the first item by default
    }

    private void NavigationViewControl_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
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

    private static Type GetPageType(SettingPageTag tag)
    {
        return tag switch
        {
            SettingPageTag.General => typeof(SettingsPaneGeneral),
            SettingPageTag.AppBar => typeof(SettingsPaneAppBar),
            SettingPageTag.About => typeof(SettingsPaneAbout),
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
    }

    private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
    {
        if (_lastItem != null)
        {
            PageHeader?.SetResourceReference(TextBlock.TextProperty, GetPageHeaderResource((SettingPageTag)_lastItem));
        }
    }

    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // Update the selected NavigationViewItem based on the page type
        NavigationViewItem? newItem;

        if (RootFrame.SourcePageType == typeof(SettingsPaneGeneral))
        {
            _lastItem = SettingPageTag.General;
            newItem = GeneralItem;
        }
        else if (RootFrame.SourcePageType == typeof(SettingsPaneAppBar))
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

        PageHeader?.SetResourceReference(TextBlock.TextProperty, GetPageHeaderResource((SettingPageTag)_lastItem));
        if (newItem != null && NavigationViewControl.SelectedItem != newItem)
        {
            NavigationViewControl.SelectedItem = newItem;
        }
    }

    private static string GetPageHeaderResource(SettingPageTag tag)
    {
        return tag switch
        {
            SettingPageTag.General => "SettingWindow_General",
            SettingPageTag.AppBar => "SettingWindow_AppBar",
            SettingPageTag.About => "SettingWindow_About",
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
    }

    #endregion
}

public enum SettingPageTag
{
    General,
    AppBar,
    About
}
