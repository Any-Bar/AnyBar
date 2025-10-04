using System.Collections;
using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Controls;


//// Implement properties for ItemsControl like behavior.
public partial class SettingsExpanderEx
{
    public IList Items
    {
        get { return (IList)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IList), typeof(SettingsExpanderEx), new PropertyMetadata(null, OnItemsConnectedPropertyChanged));

    public object ItemsSource
    {
        get { return (object)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(SettingsExpanderEx), new PropertyMetadata(null, OnItemsConnectedPropertyChanged));

    public object ItemTemplate
    {
        get { return (object)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(object), typeof(SettingsExpanderEx), new PropertyMetadata(null));

    public StyleSelector ItemContainerStyleSelector
    {
        get { return (StyleSelector)GetValue(ItemContainerStyleSelectorProperty); }
        set { SetValue(ItemContainerStyleSelectorProperty, value); }
    }

    public static readonly DependencyProperty ItemContainerStyleSelectorProperty =
        DependencyProperty.Register(nameof(ItemContainerStyleSelector), typeof(StyleSelector), typeof(SettingsExpanderEx), new PropertyMetadata(null));

    private static void OnItemsConnectedPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is SettingsExpanderEx expander && expander._itemsRepeater is not null)
        {
            var datasource = expander.ItemsSource;

            datasource ??= expander.Items;

            expander._itemsRepeater.ItemsSource = datasource;
        }
    }

    private void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (ItemContainerStyleSelector != null &&
            args.Element is FrameworkElement element &&
            element.ReadLocalValue(FrameworkElement.StyleProperty) == DependencyProperty.UnsetValue)
        {
            element.Style = ItemContainerStyleSelector.SelectStyle(null, element);
        }
    }
}
