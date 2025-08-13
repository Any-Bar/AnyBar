using System.Windows;
using System.Windows.Data;
using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Controls.Primitives;

namespace Flow.Bar.Controls;

internal sealed class CoreApplicationViewTitleBar
{
    private class Listener : DependencyObject
    {
        public static readonly DependencyProperty ExtendViewIntoTitleBarProperty = DependencyProperty.Register("ExtendViewIntoTitleBar", typeof(bool), typeof(Listener), new PropertyMetadata(OnExtendViewIntoTitleBarPropertyChanged));

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(double), typeof(Listener), new PropertyMetadata(OnHeightPropertyChanged));

        public static readonly DependencyProperty SystemOverlayLeftInsetProperty = DependencyProperty.Register("SystemOverlayLeftInset", typeof(double), typeof(Listener), new PropertyMetadata(OnSystemOverlayLeftInsetPropertyChanged));

        public static readonly DependencyProperty SystemOverlayRightInsetProperty = DependencyProperty.Register("SystemOverlayRightInset", typeof(double), typeof(Listener), new PropertyMetadata(OnSystemOverlayRightInsetPropertyChanged));

        private readonly CoreApplicationViewTitleBar _owner;

        public bool ExtendViewIntoTitleBar
        {
            get
            {
                return (bool)GetValue(ExtendViewIntoTitleBarProperty);
            }
            set
            {
                SetValue(ExtendViewIntoTitleBarProperty, value);
            }
        }

        public double Height
        {
            get
            {
                return (double)GetValue(HeightProperty);
            }
            set
            {
                SetValue(HeightProperty, value);
            }
        }

        public double SystemOverlayLeftInset
        {
            get
            {
                return (double)GetValue(SystemOverlayLeftInsetProperty);
            }
            set
            {
                SetValue(SystemOverlayLeftInsetProperty, value);
            }
        }

        public double SystemOverlayRightInset
        {
            get
            {
                return (double)GetValue(SystemOverlayRightInsetProperty);
            }
            set
            {
                SetValue(SystemOverlayRightInsetProperty, value);
            }
        }

        public Listener(CoreApplicationViewTitleBar owner)
        {
            _owner = owner;
            var owner2 = _owner._owner;
            BindingOperations.SetBinding(this, ExtendViewIntoTitleBarProperty, new Binding
            {
                Path = new PropertyPath(TitleBar.ExtendViewIntoTitleBarProperty),
                Source = owner2
            });
            BindingOperations.SetBinding(this, HeightProperty, new Binding
            {
                Path = new PropertyPath(TitleBar.HeightProperty),
                Source = owner2
            });
            BindingOperations.SetBinding(this, SystemOverlayLeftInsetProperty, new Binding
            {
                Path = new PropertyPath(TitleBar.SystemOverlayLeftInsetProperty),
                Source = owner2
            });
            BindingOperations.SetBinding(this, SystemOverlayRightInsetProperty, new Binding
            {
                Path = new PropertyPath(TitleBar.SystemOverlayRightInsetProperty),
                Source = owner2
            });
        }

        private static void OnExtendViewIntoTitleBarPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Listener)sender).OnExtendViewIntoTitleBarPropertyChanged(args);
        }

        private void OnExtendViewIntoTitleBarPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _owner.RaiseLayoutMetricsChanged();
            _owner.RaiseIsVisibleChanged();
        }

        private static void OnHeightPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Listener)sender).OnHeightPropertyChanged(args);
        }

        private void OnHeightPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _owner.RaiseLayoutMetricsChanged();
        }

        private static void OnSystemOverlayLeftInsetPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Listener)sender).OnSystemOverlayLeftInsetPropertyChanged(args);
        }

        private void OnSystemOverlayLeftInsetPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _owner.RaiseLayoutMetricsChanged();
        }

        private static void OnSystemOverlayRightInsetPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Listener)sender).OnSystemOverlayRightInsetPropertyChanged(args);
        }

        private void OnSystemOverlayRightInsetPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _owner.RaiseLayoutMetricsChanged();
        }
    }

    private static readonly DependencyProperty TitleBarProperty = DependencyProperty.RegisterAttached("TitleBar", typeof(CoreApplicationViewTitleBar), typeof(CoreApplicationViewTitleBar));

    private readonly Window _owner;

    private readonly Listener _listener;

    public bool ExtendViewIntoTitleBar
    {
        get
        {
            return TitleBar.GetExtendViewIntoTitleBar(_owner);
        }
        set
        {
            TitleBar.SetExtendViewIntoTitleBar(_owner, value);
        }
    }

    public double Height => TitleBar.GetHeight(_owner);

    public bool IsVisible => true;

    public double SystemOverlayLeftInset => TitleBar.GetSystemOverlayLeftInset(_owner);

    public double SystemOverlayRightInset => TitleBar.GetSystemOverlayRightInset(_owner);

    public event TypedEventHandler<CoreApplicationViewTitleBar, object?>? IsVisibleChanged;

    public event TypedEventHandler<CoreApplicationViewTitleBar, object?>? LayoutMetricsChanged;

    private CoreApplicationViewTitleBar(Window owner)
    {
        _owner = owner;
        _listener = new Listener(this);
    }

    private void RaiseIsVisibleChanged()
    {
        IsVisibleChanged?.Invoke(this, null);
    }

    private void RaiseLayoutMetricsChanged()
    {
        LayoutMetricsChanged?.Invoke(this, null);
    }

    internal static CoreApplicationViewTitleBar GetTitleBar(Window window)
    {
        var coreApplicationViewTitleBar = (CoreApplicationViewTitleBar)window.GetValue(TitleBarProperty);
        if (coreApplicationViewTitleBar == null)
        {
            coreApplicationViewTitleBar = new CoreApplicationViewTitleBar(window);
            SetTitleBar(window, coreApplicationViewTitleBar);
        }

        return coreApplicationViewTitleBar;
    }

    internal static CoreApplicationViewTitleBar? GetTitleBar(DependencyObject dependencyObject)
    {
        var window = Window.GetWindow(dependencyObject);
        if (window != null)
        {
            return GetTitleBar(window);
        }

        return null;
    }

    private static void SetTitleBar(Window window, CoreApplicationViewTitleBar value)
    {
        window.SetValue(TitleBarProperty, value);
    }
}
