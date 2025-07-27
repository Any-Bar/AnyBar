using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;

namespace Flow.Bar.Views;

public partial class SettingWindow : Window
{
    public SettingWindow()
    {
        InitializeComponent();
    }

    #region Window Events

    private void Window_Closed(object sender, System.EventArgs e)
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

    #endregion
}

public enum SettingPageTag
{
    AppBar,
    About
}
