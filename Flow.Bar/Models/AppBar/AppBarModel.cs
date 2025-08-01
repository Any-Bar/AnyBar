using CommunityToolkit.Mvvm.ComponentModel;
﻿using Flow.Bar.Models.Enums;
using Flow.Bar.Services;
using System;
using System.Collections.Generic;

namespace Flow.Bar.Models.AppBar;

public partial class AppBarModel : ObservableObject, IEquatable<AppBarModel>
{
    private static AppBarManagementService? _appBarManagementService;
    private static AppBarManagementService AppBarManagementService =>
        _appBarManagementService ??= Ioc.Default.GetRequiredService<AppBarManagementService>();

    public int Order { get; set; } = -1;

    [ObservableProperty]
    private bool _isEnabled = true;

    partial void OnIsEnabledChanged(bool value)
    {
        AppBarManagementService.SetEnabled(Order, value);
    }

    public string? MonitorName { get; set; } = null;

    public int? DockedWidthOrHeight { get; set; } = null;

    public bool IsResizable { get; set; } = false;

    public List<PluginControlModel> LeftOrTopPluginControls { get; set; } = [];

    public List<PluginControlModel> RightOrBottomPluginControls { get; set; } = [];

    public List<PluginControlModel> CenterPluginControls { get; set; } = [];

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
