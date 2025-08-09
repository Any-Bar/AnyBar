using System.Windows;
using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Controls.Helpers;

namespace Flow.Bar.Controls;

partial class AutoSuggestBoxEx
{
    #region UpdateTextOnSelect

    public static readonly DependencyProperty UpdateTextOnSelectProperty =
        DependencyProperty.Register(
            nameof(UpdateTextOnSelect),
            typeof(bool),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(true));

    public bool UpdateTextOnSelect
    {
        get => (bool)GetValue(UpdateTextOnSelectProperty);
        set => SetValue(UpdateTextOnSelectProperty, value);
    }

    #endregion

    #region TextMemberPath

    public static readonly DependencyProperty TextMemberPathProperty =
        DependencyProperty.Register(
            nameof(TextMemberPath),
            typeof(string),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(string.Empty));

    public string TextMemberPath
    {
        get => (string)GetValue(TextMemberPathProperty);
        set => SetValue(TextMemberPathProperty, value);
    }

    #endregion

    #region TextBoxStyle

    public static readonly DependencyProperty TextBoxStyleProperty =
        DependencyProperty.Register(
            nameof(TextBoxStyle),
            typeof(Style),
            typeof(AutoSuggestBoxEx),
            null);

    public Style TextBoxStyle
    {
        get => (Style)GetValue(TextBoxStyleProperty);
        set => SetValue(TextBoxStyleProperty, value);
    }

    #endregion

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(string.Empty, OnTextPropertyChanged, CoerceText));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((AutoSuggestBoxEx)sender).OnTextChanged(args);
    }

    private static object CoerceText(DependencyObject d, object baseValue)
    {
        return baseValue ?? string.Empty;
    }

    #endregion

    #region PlaceholderText

    public static readonly DependencyProperty PlaceholderTextProperty =
        ControlHelper.PlaceholderTextProperty.AddOwner(typeof(AutoSuggestBoxEx));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    #endregion

    #region MaxSuggestionListHeight

    public static readonly DependencyProperty MaxSuggestionListHeightProperty =
        DependencyProperty.Register(
            nameof(MaxSuggestionListHeight),
            typeof(double),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(double.PositiveInfinity));

    public double MaxSuggestionListHeight
    {
        get => (double)GetValue(MaxSuggestionListHeightProperty);
        set => SetValue(MaxSuggestionListHeightProperty, value);
    }

    #endregion

    #region IsSuggestionListOpen

    public static readonly DependencyProperty IsSuggestionListOpenProperty =
        DependencyProperty.Register(
            nameof(IsSuggestionListOpen),
            typeof(bool),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(false, OnIsSuggestionListOpenPropertyChanged));

    public bool IsSuggestionListOpen
    {
        get => (bool)GetValue(IsSuggestionListOpenProperty);
        set => SetValue(IsSuggestionListOpenProperty, value);
    }

    private static void OnIsSuggestionListOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((AutoSuggestBoxEx)sender).OnIsSuggestionListOpenChanged(args);
    }

    #endregion

    #region Header

    public static readonly DependencyProperty HeaderProperty =
        ControlHelper.HeaderProperty.AddOwner(typeof(AutoSuggestBoxEx));

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    #endregion

    #region QueryIcon

    public static readonly DependencyProperty QueryIconProperty =
        DependencyProperty.Register(
            nameof(QueryIcon),
            typeof(object),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(null, OnQueryIconPropertyChanged));

    public object QueryIcon
    {
        get => (object)GetValue(QueryIconProperty);
        set => SetValue(QueryIconProperty, value);
    }

    private static void OnQueryIconPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((AutoSuggestBoxEx)sender).OnQueryIconChanged(args);
    }

    #endregion

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        ControlHelper.DescriptionProperty.AddOwner(typeof(AutoSuggestBoxEx));

    public object Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion

    #region UseSystemFocusVisuals

    public static readonly DependencyProperty UseSystemFocusVisualsProperty =
        FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(AutoSuggestBoxEx));

    public bool UseSystemFocusVisuals
    {
        get => (bool)GetValue(UseSystemFocusVisualsProperty);
        set => SetValue(UseSystemFocusVisualsProperty, value);
    }

    #endregion

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        ControlHelper.CornerRadiusProperty.AddOwner(typeof(AutoSuggestBoxEx));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region AllowQuerySubmit

    public static readonly DependencyProperty AllowQuerySubmitProperty =
        DependencyProperty.Register(
            nameof(AllowQuerySubmit),
            typeof(bool),
            typeof(AutoSuggestBoxEx),
            new PropertyMetadata(true));

    public bool AllowQuerySubmit
    {
        get => (bool)GetValue(AllowQuerySubmitProperty);
        set => SetValue(AllowQuerySubmitProperty, value);
    }

    #endregion

    public event TypedEventHandler<AutoSuggestBoxEx, AutoSuggestBoxExSuggestionChosenEventArgs>? SuggestionChosen;

    public event TypedEventHandler<AutoSuggestBoxEx, AutoSuggestBoxExTextChangedEventArgs>? TextChanged;

    public event TypedEventHandler<AutoSuggestBoxEx, AutoSuggestBoxExQuerySubmittedEventArgs>? QuerySubmitted;
}
