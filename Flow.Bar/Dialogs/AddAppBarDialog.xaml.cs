using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Controls;
using Flow.Bar.Helper.Monitor;
using Flow.Bar.Models.Enums;
using Flow.Bar.Models.Monitor;
using System;
using System.Collections.Generic;

namespace Flow.Bar.Dialogs;

[INotifyPropertyChanged]
public partial class AddAppBarDialog : ContentDialog
{
    private static readonly string ClassName = nameof(AddAppBarDialog);

    [ObservableProperty]
    private string _appBarName = string.Empty;

    public List<AppBarDockModeLocalized> AllDockModes { get; } = AppBarDockModeLocalized.GetValues();

    [ObservableProperty]
    private AppBarDockMode _dockMode = AppBarDockMode.Top;

    partial void OnDockModeChanged(AppBarDockMode value)
    {
        UpdateMinAndMaxDockedWidthOrHeight();
    }

    public List<MonitorNameLocalized> AllMonitorNames { get; } = MonitorNameLocalized.GetValues(null);

    [ObservableProperty]
    private MonitorInfo _actualMonitor = null!;

    partial void OnActualMonitorChanged(MonitorInfo value)
    {
        UpdateMinAndMaxDockedWidthOrHeight();
    }

    [ObservableProperty]
    private string? _monitorName = null;

    partial void OnMonitorNameChanged(string? value)
    {
        UpdateActualMonitor(value);
    }

    [ObservableProperty]
    private bool _followSystemTaskbarWidthOrHeight = true;

    [ObservableProperty]
    private int _minDockedWidthOrHeight = 0;

    [ObservableProperty]
    private int _maxDockedWidthOrHeight = int.MaxValue;

    [ObservableProperty]
    private int _dockedWidthOrHeight = MonitorInfoHelper.DefaultDockedWidthOrHeight;

    [ObservableProperty]
    private bool _isResizable = false;
    
    public AddAppBarDialog()
    {
        UpdateMinAndMaxDockedWidthOrHeight();
        InitializeComponent();
    }

    private void UpdateActualMonitor(string? monitorName)
    {
        var monitor = MonitorInfoHelper.GetMonitorInfoFromName(monitorName);
        if (monitor != null)
        {
            ActualMonitor = monitor;
        }
        else
        {
            App.API.LogError(ClassName, $"Monitor not found: <{monitorName}>");
        }
    }

    private void UpdateMinAndMaxDockedWidthOrHeight()
    {
        if (ActualMonitor == null)
        {
            UpdateActualMonitor(MonitorName);
            ArgumentNullException.ThrowIfNull(ActualMonitor);
        }
        var dockedWidthOrHeight = DockedWidthOrHeight;
        (MinDockedWidthOrHeight, MaxDockedWidthOrHeight, DockedWidthOrHeight) =
            MonitorInfoHelper.GetMinAndMaxDockedWidthOrHeight(dockedWidthOrHeight, DockMode, ActualMonitor);
    }
}
