using Flow.Bar.Interfaces.Enumerable;
using Flow.Bar.Models.Enums;
using System;
using System.Collections.Generic;

namespace Flow.Bar.Models.AppBar;

public class AppBarModel : IEquatable<AppBarModel>, IOrder
{
    public int Order { get; set; } = -1;

    public string Name { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public AppBarDockMode DockMode { get; set; } = AppBarDockMode.Top;

    public string? MonitorName { get; set; } = null;

    public bool FollowSystemTaskbarWidthOrHeight { get; set; } = true;

    public int DockedWidthOrHeight { get; set; } = 0;

    public bool IsResizable { get; set; } = false;

    private List<BarElementModel> _leftOrTopBarElements = [];
    [Obsolete("This property is for storage only. Please use AppBarManagementService instead of calling this property directly.")]
    public List<BarElementModel> LeftOrTopBarElements
    {
        get
        {
            foreach (var element in _leftOrTopBarElements)
            {
                element.AppBar = this;
                element.BarElementPosition = BarElementModelPosition.LeftOrTop;
            }
            return _leftOrTopBarElements;
        }
        set
        {
            _leftOrTopBarElements = value;
            foreach (var element in _leftOrTopBarElements)
            {
                element.AppBar = this;
                element.BarElementPosition = BarElementModelPosition.LeftOrTop;
            }
        }
    }

    private List<BarElementModel> _centerBarElements = [];
    [Obsolete("This property is for storage only. Please use AppBarManagementService instead of calling this property directly.")]
    public List<BarElementModel> CenterBarElements
    {
        get
        {
            foreach (var element in _centerBarElements)
            {
                element.AppBar = this;
                element.BarElementPosition = BarElementModelPosition.Center;
            }
            return _centerBarElements;
        }
        set
        {
            _centerBarElements = value;
            foreach (var element in _centerBarElements)
            {
                element.AppBar = this;
                element.BarElementPosition = BarElementModelPosition.Center;
            }
        }
    }

    private List<BarElementModel> _rightOrBottomBarElements = [];
    [Obsolete("This property is for storage only. Please use AppBarManagementService instead of calling this property directly.")]
    public List<BarElementModel> RightOrBottomBarElements
    {
        get
        {
            foreach (var element in _rightOrBottomBarElements)
            {
                element.AppBar = this;
                element.BarElementPosition = BarElementModelPosition.RightOrBottom;
            }
            return _rightOrBottomBarElements;
        }
        set
        {
            _rightOrBottomBarElements = value;
            foreach (var element in _rightOrBottomBarElements)
            {
                element.AppBar = this;
                element.BarElementPosition = BarElementModelPosition.RightOrBottom;
            }
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as AppBarModel);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Order.GetHashCode();

    /// <inheritdoc />
    public bool Equals(AppBarModel? other) => Order == other?.Order;

    /// <inheritdoc />
    public static bool operator ==(AppBarModel? a, AppBarModel? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    /// <inheritdoc />
    public static bool operator !=(AppBarModel? a, AppBarModel? b) => !(a == b);
}
