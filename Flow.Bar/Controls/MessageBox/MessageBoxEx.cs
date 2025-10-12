using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using iNKORE.UI.WPF.Helpers;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;

namespace Flow.Bar.Controls;

/// <summary>
/// Custom MessageBox from UI.WPF.Modern 0.10.1 with colorful icon support & layout fixes.
/// </summary>
public partial class MessageBoxEx : Window
{
    private MessageBoxResult? _result;
    public MessageBoxResult Result
    {
        get => _result ?? MessageBoxResult.None;
    }

    public Button? OKButton { get; private set; }
    public Button? YesButton { get; private set; }
    public Button? NoButton { get; private set; }
    public Button? CancelButton { get; private set; }
    public Border? Border_UpperBackground { get; private set; }

    public static BackdropType DefaultBackdropType { get; set; } = BackdropType.Mica;

    public static bool MakeSound { get; set; } = true;

    static MessageBoxEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageBoxEx), new FrameworkPropertyMetadata(typeof(MessageBoxEx)));
    }

    public static readonly DependencyPropertyDescriptor SystemBackdropTypeProperty_Descriptor = DependencyPropertyDescriptor.FromProperty(WindowHelper.SystemBackdropTypeProperty, typeof(MessageBoxEx));

    public MessageBoxEx()
    {
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(ExecuteCopy)));

        SetValue(TemplateSettingsPropertyKey, new MessageBoxExTemplateSettings());
        var handler = new RoutedEventHandler((sender, e) => ApplyDarkMode());
        ThemeManager.AddActualThemeChangedHandler(this, handler);

        Loaded += On_Loaded;

        SystemBackdropTypeProperty_Descriptor.AddValueChanged(this, SystemBackdropTypeProperty_ValueChanged);
        ThemeManager.AddActualThemeChangedHandler(this, ThemeManager_AddActualThemeChanged);
    }

    private void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
    {
        const string longlines = "---------------------------";
        StringBuilder sb = new();
        sb.Append(longlines);
        sb.AppendLine();
        sb.Append(Caption);
        sb.AppendLine();
        sb.Append(longlines);
        sb.AppendLine();
        sb.Append(Content);
        sb.AppendLine();
        sb.Append(longlines);
        sb.AppendLine();

        var isFirstButtonLoaded = true;
        var buttons = new Button?[]
        {
            OKButton,
            YesButton,
            NoButton,
            CancelButton,
        };

        foreach (var button in buttons)
        {
            if (button?.Visibility == Visibility.Visible)
            {
                if (!isFirstButtonLoaded)
                {
                    sb.Append("     ");
                }

                sb.Append(button.Content.ToString());
                isFirstButtonLoaded = false;
            }
        }

        sb.AppendLine();
        sb.Append(longlines);

        try
        {
#pragma warning disable SYSLIB0003 // Type or member is obsolete
            new UIPermission(UIPermissionClipboard.AllClipboard).Demand();
#pragma warning restore SYSLIB0003 // Type or member is obsolete
            ClipboardEx.SetText(sb.ToString());
        }
        catch (SecurityException)
        {
            if (Debugger.IsAttached)
            {
                throw;
            }
        }
    }

    private void ThemeManager_AddActualThemeChanged(object? sender, RoutedEventArgs e)
    {
        if (WindowHelper.GetSystemBackdropType(this) != BackdropType.None)
        {
            if (ThemeManager.GetActualTheme(this) == ElementTheme.Dark)
            {
                BackdropHelper.ApplyDarkMode(this);
            }
            else
            {
                BackdropHelper.RemoveDarkMode(this);
            }
        }
    }

    private void SystemBackdropTypeProperty_ValueChanged(object? sender, EventArgs e)
    {
        var backdrop = WindowHelper.GetSystemBackdropType(this);

        if (ReadLocalValue(BackgroundProperty) == DependencyProperty.UnsetValue)
        {
            if (backdrop == BackdropType.None || !backdrop.IsSupported())
            {
                SetResourceReference(BackgroundProperty, ThemeKeys.ContentDialogBackgroundKey);
                Border_UpperBackground?.SetResourceReference(BackgroundProperty, ThemeKeys.ContentDialogTopOverlayKey);
            }
            else
            {
                Background = Brushes.Transparent;
                Border_UpperBackground?.SetResourceReference(BackgroundProperty, ThemeKeys.LayerOnAcrylicFillColorDefaultBrushKey);
            }
        }
    }

    #region SystemSoundOnLoaded

    public static readonly DependencyProperty SystemSoundOnLoadedProperty =
        DependencyProperty.Register(
            nameof(SystemSoundOnLoaded),
            typeof(SystemSound),
            typeof(MessageBoxEx));

    public SystemSound? SystemSoundOnLoaded
    {
        get => (SystemSound?)GetValue(SystemSoundOnLoadedProperty);
        set => SetValue(SystemSoundOnLoadedProperty, value);
    }

    #endregion

    #region Caption

    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(
            nameof(Caption),
            typeof(object),
            typeof(MessageBoxEx));

    public object Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    #endregion

    #region CaptionTemplate

    public static readonly DependencyProperty CaptionTemplateProperty =
        DependencyProperty.Register(
            nameof(CaptionTemplate),
            typeof(DataTemplate),
            typeof(MessageBoxEx));

    public DataTemplate CaptionTemplate
    {
        get => (DataTemplate)GetValue(CaptionTemplateProperty);
        set => SetValue(CaptionTemplateProperty, value);
    }

    #endregion

    #region OKButtonText

    public static readonly DependencyProperty OKButtonTextProperty =
        DependencyProperty.Register(
            nameof(OKButtonText),
            typeof(string),
            typeof(MessageBoxEx),
            new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string OKButtonText
    {
        get => (string)GetValue(OKButtonTextProperty);
        set => SetValue(OKButtonTextProperty, value);
    }

    #endregion

    #region OKButtonCommand

    public static readonly DependencyProperty OKButtonCommandProperty =
        DependencyProperty.Register(
            nameof(OKButtonCommand),
            typeof(ICommand),
            typeof(MessageBoxEx),
            null);

    public ICommand OKButtonCommand
    {
        get => (ICommand)GetValue(OKButtonCommandProperty);
        set => SetValue(OKButtonCommandProperty, value);
    }

    #endregion

    #region OKButtonCommandParameter

    public static readonly DependencyProperty OKButtonCommandParameterProperty =
        DependencyProperty.Register(
            nameof(OKButtonCommandParameter),
            typeof(object),
            typeof(MessageBoxEx),
            null);

    public object OKButtonCommandParameter
    {
        get => GetValue(OKButtonCommandParameterProperty);
        set => SetValue(OKButtonCommandParameterProperty, value);
    }

    #endregion

    #region OKButtonStyle

    public static readonly DependencyProperty OKButtonStyleProperty =
        DependencyProperty.Register(
            nameof(OKButtonStyle),
            typeof(Style),
            typeof(MessageBoxEx),
            null);

    public Style OKButtonStyle
    {
        get => (Style)GetValue(OKButtonStyleProperty);
        set => SetValue(OKButtonStyleProperty, value);
    }

    #endregion

    #region YesButtonText

    public static readonly DependencyProperty YesButtonTextProperty =
        DependencyProperty.Register(
            nameof(YesButtonText),
            typeof(string),
            typeof(MessageBoxEx),
            new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string YesButtonText
    {
        get => (string)GetValue(YesButtonTextProperty);
        set => SetValue(YesButtonTextProperty, value);
    }

    #endregion

    #region YesButtonCommand

    public static readonly DependencyProperty YesButtonCommandProperty =
        DependencyProperty.Register(
            nameof(YesButtonCommand),
            typeof(ICommand),
            typeof(MessageBoxEx),
            null);

    public ICommand YesButtonCommand
    {
        get => (ICommand)GetValue(YesButtonCommandProperty);
        set => SetValue(YesButtonCommandProperty, value);
    }

    #endregion

    #region YesButtonCommandParameter

    public static readonly DependencyProperty YesButtonCommandParameterProperty =
        DependencyProperty.Register(
            nameof(YesButtonCommandParameter),
            typeof(object),
            typeof(MessageBoxEx),
            null);

    public object YesButtonCommandParameter
    {
        get => GetValue(YesButtonCommandParameterProperty);
        set => SetValue(YesButtonCommandParameterProperty, value);
    }

    #endregion

    #region YesButtonStyle

    public static readonly DependencyProperty YesButtonStyleProperty =
        DependencyProperty.Register(
            nameof(YesButtonStyle),
            typeof(Style),
            typeof(MessageBoxEx),
            null);

    public Style YesButtonStyle
    {
        get => (Style)GetValue(YesButtonStyleProperty);
        set => SetValue(YesButtonStyleProperty, value);
    }

    #endregion

    #region NoButtonText

    public static readonly DependencyProperty NoButtonTextProperty =
        DependencyProperty.Register(
            nameof(NoButtonText),
            typeof(string),
            typeof(MessageBoxEx),
            new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string NoButtonText
    {
        get => (string)GetValue(NoButtonTextProperty);
        set => SetValue(NoButtonTextProperty, value);
    }

    #endregion

    #region NoButtonCommand

    public static readonly DependencyProperty NoButtonCommandProperty =
        DependencyProperty.Register(
            nameof(NoButtonCommand),
            typeof(ICommand),
            typeof(MessageBoxEx),
            null);

    public ICommand NoButtonCommand
    {
        get => (ICommand)GetValue(NoButtonCommandProperty);
        set => SetValue(NoButtonCommandProperty, value);
    }

    #endregion

    #region NoButtonCommandParameter

    public static readonly DependencyProperty NoButtonCommandParameterProperty =
        DependencyProperty.Register(
            nameof(NoButtonCommandParameter),
            typeof(object),
            typeof(MessageBoxEx),
            null);

    public object NoButtonCommandParameter
    {
        get => GetValue(NoButtonCommandParameterProperty);
        set => SetValue(NoButtonCommandParameterProperty, value);
    }

    #endregion

    #region NoButtonStyle

    public static readonly DependencyProperty NoButtonStyleProperty =
        DependencyProperty.Register(
            nameof(NoButtonStyle),
            typeof(Style),
            typeof(MessageBoxEx),
            null);

    public Style NoButtonStyle
    {
        get => (Style)GetValue(NoButtonStyleProperty);
        set => SetValue(NoButtonStyleProperty, value);
    }

    #endregion

    #region CancelButtonText

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(
            nameof(CancelButtonText),
            typeof(string),
            typeof(MessageBoxEx),
            new PropertyMetadata(string.Empty, OnButtonTextChanged));

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    #endregion

    #region CancelButtonCommand

    public static readonly DependencyProperty CancelButtonCommandProperty =
        DependencyProperty.Register(
            nameof(CancelButtonCommand),
            typeof(ICommand),
            typeof(MessageBoxEx),
            null);

    public ICommand CancelButtonCommand
    {
        get => (ICommand)GetValue(CancelButtonCommandProperty);
        set => SetValue(CancelButtonCommandProperty, value);
    }

    #endregion

    #region CancelButtonCommandParameter

    public static readonly DependencyProperty CancelButtonCommandParameterProperty =
        DependencyProperty.Register(
            nameof(CancelButtonCommandParameter),
            typeof(object),
            typeof(MessageBoxEx),
            null);

    public object CancelButtonCommandParameter
    {
        get => GetValue(CancelButtonCommandParameterProperty);
        set => SetValue(CancelButtonCommandParameterProperty, value);
    }

    #endregion

    #region CancelButtonStyle

    public static readonly DependencyProperty CancelButtonStyleProperty =
        DependencyProperty.Register(
            nameof(CancelButtonStyle),
            typeof(Style),
            typeof(MessageBoxEx),
            null);

    public Style CancelButtonStyle
    {
        get => (Style)GetValue(CancelButtonStyleProperty);
        set => SetValue(CancelButtonStyleProperty, value);
    }

    #endregion

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(MessageBoxEx));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region ImageSource

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(MessageBoxEx),
            new PropertyMetadata(OnImageSourcePropertyChanged));

    private static void OnImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((MessageBoxEx)sender).OnImageSourcePropertyChanged(args);
    }

    #endregion

    #region MessageBoxButtons

    public MessageBoxButton MessageBoxButtons
    {
        get => (MessageBoxButton)GetValue(MessageBoxButtonsProperty);
        set => SetValue(MessageBoxButtonsProperty, value);
    }

    public static readonly DependencyProperty MessageBoxButtonsProperty =
        DependencyProperty.Register(
            nameof(MessageBoxButtons),
            typeof(MessageBoxButton),
            typeof(MessageBoxEx),
            new PropertyMetadata(OnMessageBoxButtonsPropertyChanged));

    private static void OnMessageBoxButtonsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((MessageBoxEx)sender).UpdateMessageBoxButtonState();
    }

    #endregion

    #region DefaultResult

    public MessageBoxResult? DefaultResult
    {
        get => (MessageBoxResult?)GetValue(DefaultResultProperty);
        set => SetValue(DefaultResultProperty, value);
    }

    public static readonly DependencyProperty DefaultResultProperty =
        DependencyProperty.Register(
            nameof(DefaultResult),
            typeof(MessageBoxResult?),
            typeof(MessageBoxEx));

    #endregion

    #region TemplateSettings

    private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(TemplateSettings),
            typeof(MessageBoxExTemplateSettings),
            typeof(MessageBoxEx),
            null);

    public static readonly DependencyProperty TemplateSettingsProperty =
        TemplateSettingsPropertyKey.DependencyProperty;

    public MessageBoxExTemplateSettings TemplateSettings
    {
        get => (MessageBoxExTemplateSettings)GetValue(TemplateSettingsProperty);
    }

    #endregion

    public event TypedEventHandler<MessageBoxEx, MessageBoxExOpenedEventArgs>? Opened;

    public new event TypedEventHandler<MessageBoxEx, MessageBoxExClosingEventArgs>? Closing;

    public new event TypedEventHandler<MessageBoxEx, MessageBoxExClosedEventArgs>? Closed;

    public event TypedEventHandler<MessageBoxEx, MessageBoxExButtonClickEventArgs>? OKButtonClick;

    public event TypedEventHandler<MessageBoxEx, MessageBoxExButtonClickEventArgs>? YesButtonClick;

    public event TypedEventHandler<MessageBoxEx, MessageBoxExButtonClickEventArgs>? NoButtonClick;

    public event TypedEventHandler<MessageBoxEx, MessageBoxExButtonClickEventArgs>? CancelButtonClick;

    private static void OnButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MessageBoxEx)d).UpdateButtonTextState();
    }

    private void OnImageSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is ImageSource imageSource)
        {
            TemplateSettings.IconElement = imageSource;
        }
        else
        {
            TemplateSettings.ClearValue(MessageBoxExTemplateSettings.IconElementProperty);
        }
        UpdateIconState();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (OKButton != null)
        {
            OKButton.Click -= OnButtonClick;
        }

        if (YesButton != null)
        {
            YesButton.Click -= OnButtonClick;
        }

        if (NoButton != null)
        {
            NoButton.Click -= OnButtonClick;
        }

        if (CancelButton != null)
        {
            CancelButton.Click -= OnButtonClick;
        }

        OKButton = GetTemplateChild(nameof(OKButton)) as Button;
        YesButton = GetTemplateChild(nameof(YesButton)) as Button;
        NoButton = GetTemplateChild(nameof(NoButton)) as Button;
        CancelButton = GetTemplateChild(nameof(CancelButton)) as Button;
        Border_UpperBackground = GetTemplateChild(nameof(Border_UpperBackground)) as Border;

        if (OKButton != null)
        {
            OKButton.Click += OnButtonClick;
        }

        if (YesButton != null)
        {
            YesButton.Click += OnButtonClick;
        }

        if (NoButton != null)
        {
            NoButton.Click += OnButtonClick;
        }

        if (CancelButton != null)
        {
            CancelButton.Click += OnButtonClick;
            CancelButton.IsCancel = true;
        }

        UpdateIconState();
        UpdateMessageState();
        UpdateButtonTextState();
        UpdateMessageBoxButtonState();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        InvalidateMeasure();
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender == OKButton)
        {
            HandleButtonClick(
                OKButtonClick,
                OKButtonCommand,
                OKButtonCommandParameter,
                MessageBoxResult.OK);
        }
        else if (sender == YesButton)
        {
            HandleButtonClick(
                YesButtonClick,
                YesButtonCommand,
                YesButtonCommandParameter,
                MessageBoxResult.Yes);
        }
        else if (sender == NoButton)
        {
            HandleButtonClick(
                NoButtonClick,
                NoButtonCommand,
                NoButtonCommandParameter,
                MessageBoxResult.No);
        }
        else if (sender == CancelButton)
        {
            HandleButtonClick(
                CancelButtonClick,
                CancelButtonCommand,
                CancelButtonCommandParameter,
                MessageBoxResult.Cancel);
        }
    }

    private void HandleButtonClick(
        TypedEventHandler<MessageBoxEx, MessageBoxExButtonClickEventArgs>? handler,
        ICommand command,
        object commandParameter,
        MessageBoxResult result)
    {
        if (handler != null)
        {
            var args = new MessageBoxExButtonClickEventArgs();

            var deferral = new MessageBoxExButtonClickDeferral(() =>
            {
                if (!args.Cancel)
                {
                    TryExecuteCommand(command, commandParameter);
                    Close(result);
                }
            });

            args.SetDeferral(deferral);

            args.IncrementDeferralCount();
            handler(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            TryExecuteCommand(command, commandParameter);
            Close(result);
        }
    }

    private void UpdateButtonTextState()
    {
        var templateSettings = TemplateSettings;
        templateSettings.OKButtonText = string.IsNullOrEmpty(OKButtonText) ? Localize.MessageBoxEx_Ok() : OKButtonText;
        templateSettings.YesButtonText = string.IsNullOrEmpty(YesButtonText) ? Localize.MessageBoxEx_Yes() : YesButtonText;
        templateSettings.NoButtonText = string.IsNullOrEmpty(NoButtonText) ? Localize.MessageBoxEx_No() : NoButtonText;
        templateSettings.CancelButtonText = string.IsNullOrEmpty(CancelButtonText) ? Localize.MessageBoxEx_Cancel() : CancelButtonText;
    }

    private void UpdateMessageState()
    {
        var stateName = Caption == null || (Caption is string str && string.IsNullOrEmpty(str)) ? TitleCollapsedState : TitleVisibleState;
        VisualStateManager.GoToState(this, stateName, true);
    }

    private void UpdateIconState()
    {
        var stateName = TemplateSettings.IconElement == null ? IconCollapsedState : IconVisibleState;
        VisualStateManager.GoToState(this, stateName, true);
    }

    private void UpdateMessageBoxButtonState()
    {
        string stateName;

        var button = MessageBoxButtons;

        switch (button)
        {
            case MessageBoxButton.OK:
                stateName = OKVisibleState;
                OKButton?.Focus();
                break;
            case MessageBoxButton.OKCancel:
                stateName = OKCancelVisibleState;
                OKButton?.Focus();
                break;
            case MessageBoxButton.YesNoCancel:
                stateName = YesNoCancelVisibleState;
                YesButton?.Focus();
                break;
            case MessageBoxButton.YesNo:
                stateName = YesNoVisibleState;
                YesButton?.Focus();
                break;
            default:
                stateName = OKVisibleState;
                OKButton?.Focus();
                break;
        }

        VisualStateManager.GoToState(this, stateName, true);

        if (_result == null)
        {
            stateName = button switch
            {
                MessageBoxButton.OK => OKAsDefaultButtonState,
                MessageBoxButton.OKCancel => OKAsDefaultButtonState,
                MessageBoxButton.YesNoCancel => YesAsDefaultButtonState,
                MessageBoxButton.YesNo => YesAsDefaultButtonState,
                _ => OKAsDefaultButtonState,
            };
        }
        else
        {
            stateName = _result.Value switch
            {
                MessageBoxResult.OK => OKAsDefaultButtonState,
                MessageBoxResult.Cancel => CancelAsDefaultButtonState,
                MessageBoxResult.Yes => YesAsDefaultButtonState,
                MessageBoxResult.No => NoAsDefaultButtonState,
                _ => NoneAsDefaultButtonState,
            };
        }

        VisualStateManager.GoToState(this, stateName, true);
    }

    /// <summary>
    /// Opens a Message Box and returns only when the newly opened window is closed.
    /// </summary>
    /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public new MessageBoxResult ShowDialog()
    {
        base.ShowDialog();
        return Result;
    }

    public void Close(MessageBoxResult result)
    {
        var closing = Closing;
        if (closing != null)
        {
            var args = new MessageBoxExClosingEventArgs(result);

            var deferral = new MessageBoxExClosingDeferral(() =>
            {
                if (!args.Cancel)
                {
                    _result = result;
                    Close();
                    Closed?.Invoke(this, new MessageBoxExClosedEventArgs(result));
                }
            });

            args.SetDeferral(deferral);

            args.IncrementDeferralCount();
            closing(this, args);
            args.DecrementDeferralCount();
        }
        else
        {
            _result = result;
            Close();
            Closed?.Invoke(this, new MessageBoxExClosedEventArgs(result));
        }
    }

    private void On_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyDarkMode();
        this.RemoveTitleBar();
        SetWindowUndraggable();
        Opened?.Invoke(this, new MessageBoxExOpenedEventArgs());

        if (DefaultBackdropType == BackdropType.None || BackdropHelper.IsSupported(DefaultBackdropType))
        {
            WindowHelper.SetSystemBackdropType(this, DefaultBackdropType);
        }

        ThemeManager_AddActualThemeChanged(sender, e);
        SystemBackdropTypeProperty_ValueChanged(sender, e);

        SystemSoundOnLoaded?.Play();
    }

    private void SetWindowUndraggable()
    {
        var draggableChrome = WindowChrome.GetWindowChrome(this);
        var nonDraggableChrome = new WindowChrome
        {
            NonClientFrameEdges = draggableChrome.NonClientFrameEdges,
            CaptionHeight = 0,  // Disable caption height for non-draggable chrome
            CornerRadius = draggableChrome.CornerRadius,
            GlassFrameThickness = draggableChrome.GlassFrameThickness,
            ResizeBorderThickness = draggableChrome.ResizeBorderThickness,
            UseAeroCaptionButtons = draggableChrome.UseAeroCaptionButtons
        };
        WindowChrome.SetWindowChrome(this, nonDraggableChrome);
    }

    private static void TryExecuteCommand(ICommand command, object parameter)
    {
        if (command != null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    private void ApplyDarkMode()
    {
        var theme = ThemeManager.GetActualTheme(this);

        static bool IsDark(ElementTheme theme)
        {
            return theme == ElementTheme.Default
                ? ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark
                : theme == ElementTheme.Dark;
        }

        if (IsDark(theme))
        {
            BackdropHelper.ApplyDarkMode(this);
        }
        else
        {
            this.RemoveDarkMode();
        }
    }

    private const string OKVisibleState = "OKVisible";
    private const string OKCancelVisibleState = "OKCancelVisible";
    private const string YesNoCancelVisibleState = "YesNoCancelVisible";
    private const string YesNoVisibleState = "YesNoVisible";

    private const string OKAsDefaultButtonState = "OKAsDefaultButton";
    private const string YesAsDefaultButtonState = "YesAsDefaultButton";
    private const string CancelAsDefaultButtonState = "CancelAsDefaultButton";
    private const string NoAsDefaultButtonState = "NoAsDefaultButton";
    private const string NoneAsDefaultButtonState = "NoneAsDefaultButton";

    private const string IconVisibleState = "IconVisible";
    private const string IconCollapsedState = "IconCollapsed";

    private const string TitleVisibleState = "TitleVisible";
    private const string TitleCollapsedState = "TitleCollapsed";
}
