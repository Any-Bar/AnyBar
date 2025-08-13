using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Controls;

public class MessageBoxExTemplateSettings : DependencyObject
{
    internal MessageBoxExTemplateSettings()
    {
    }

    #region IconElement

    private static readonly DependencyPropertyKey IconElementPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IconElement),
            typeof(IconElement),
            typeof(MessageBoxExTemplateSettings),
            null);

    public static readonly DependencyProperty IconElementProperty = IconElementPropertyKey.DependencyProperty;

    public IconElement IconElement
    {
        get => (IconElement)GetValue(IconElementProperty);
        internal set => SetValue(IconElementPropertyKey, value);
    }

    #endregion

    #region OKButtonText

    public static readonly DependencyProperty OKButtonTextProperty =
        DependencyProperty.Register(
            nameof(OKButtonText),
            typeof(string),
            typeof(MessageBoxExTemplateSettings),
            new PropertyMetadata(null));

    public string OKButtonText
    {
        get => (string)GetValue(OKButtonTextProperty);
        set => SetValue(OKButtonTextProperty, value);
    }

    #endregion

    #region YesButtonText

    public static readonly DependencyProperty YesButtonTextProperty =
        DependencyProperty.Register(
            nameof(YesButtonText),
            typeof(string),
            typeof(MessageBoxExTemplateSettings),
            new PropertyMetadata(null));

    public string YesButtonText
    {
        get => (string)GetValue(YesButtonTextProperty);
        set => SetValue(YesButtonTextProperty, value);
    }

    #endregion

    #region NoButtonText

    public static readonly DependencyProperty NoButtonTextProperty =
        DependencyProperty.Register(
            nameof(NoButtonText),
            typeof(string),
            typeof(MessageBoxExTemplateSettings),
            new PropertyMetadata(null));

    public string NoButtonText
    {
        get => (string)GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }

    #endregion

    #region CancelButtonText

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(
            nameof(CancelButtonText),
            typeof(string),
            typeof(MessageBoxExTemplateSettings),
            new PropertyMetadata(null));

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    #endregion
}
