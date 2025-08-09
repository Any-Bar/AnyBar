using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iNKORE.UI.WPF.Helpers;

namespace Flow.Bar.Controls;

public class AutoSuggestBoxExListView : ListView
{
    static AutoSuggestBoxExListView()
    {
        SelectionModeProperty.OverrideMetadata(typeof(AutoSuggestBoxExListView), new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    #region IsItemClickEnabled

    public static readonly DependencyProperty IsItemClickEnabledProperty =
        DependencyProperty.Register(
            nameof(IsItemClickEnabled),
            typeof(bool),
            typeof(AutoSuggestBoxExListView),
            new PropertyMetadata(false));

    public bool IsItemClickEnabled
    {
        get => (bool)GetValue(IsItemClickEnabledProperty);
        set => SetValue(IsItemClickEnabledProperty, value);
    }

    #endregion

    public event AutoSuggestBoxExItemClickEventHandler? ItemClick;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        m_scrollHost = this.FindDescendant<ScrollViewer>();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is AutoSuggestBoxExListViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new AutoSuggestBoxExListViewItem();
    }

    internal void NotifyListItemClicked(AutoSuggestBoxExListViewItem item, MouseButton? mouseButton = null)
    {
        if (IsItemClickEnabled)
        {
            OnItemClick(item);
        }

        switch (SelectionMode)
        {
            case SelectionMode.Single:
                {
                    if (!item.IsSelected)
                    {
                        item.SetCurrentValue(IsSelectedProperty, true);
                    }
                    else if (mouseButton.HasValue && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        item.SetCurrentValue(IsSelectedProperty, false);
                    }
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    internal void ScrollToTop()
    {
        m_scrollHost?.ScrollToTop();
    }

    private void OnItemClick(AutoSuggestBoxExListViewItem lvi)
    {
        var item = ItemContainerGenerator.ItemFromContainer(lvi);
        if (item != null)
        {
            ItemClick?.Invoke(this, new AutoSuggestBoxExItemClickEventArgs { ClickedItem = item });
        }
    }

    private ScrollViewer? m_scrollHost;
}
