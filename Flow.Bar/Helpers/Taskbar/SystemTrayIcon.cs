// Copyright (c) Files Community
// Licensed under the MIT License.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using RoutedEventHandler = System.Windows.RoutedEventHandler;

namespace Flow.Bar.Helpers.Taskbar;

/// <summary>
/// Represents a tray icon of Notification Area so-called System Tray.
/// </summary>
public sealed partial class SystemTrayIcon : IDisposable
{
    // Constants

    private const uint WM_FILES_UNIQUE_MESSAGE = 2048u;

    // Fields

    private static readonly string ClassName = nameof(SystemTrayIcon);

    private static readonly Guid _trayIconGuid = new("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

    private readonly SystemTrayIconWindow _IconWindow;

    private readonly uint _taskbarRestartMessageId;

    private bool _notifyIconCreated;

    // Properties

    public Guid Id { get; private set; }

    private bool _IsVisible;
    public bool IsVisible
    {
        get
        {
            return _IsVisible;
        }
        private set
        {
            if (_IsVisible != value)
            {
                _IsVisible = value;

                if (!value)
                {
                    DeleteNotifyIcon();
                }
                else
                {
                    CreateOrModifyNotifyIcon();
                }
            }
        }
    }

    private string _Tooltip = string.Empty;
    public string Tooltip
    {
        get
        {
            return _Tooltip;
        }
        set
        {
            if (_Tooltip != value)
            {
                _Tooltip = value;

                CreateOrModifyNotifyIcon();
            }
        }
    }

    private Icon _Icon = null!;
    public Icon Icon
    {
        get
        {
            return _Icon;
        }
        set
        {
            if (_Icon != value)
            {
                _Icon = value;

                CreateOrModifyNotifyIcon();
            }
        }
    }

    public Rect Position
    {
        get
        {
            if (!IsVisible)
            {
                return default;
            }

            NOTIFYICONIDENTIFIER identifier = default;
            identifier.cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONIDENTIFIER));
            identifier.hWnd = _IconWindow.WindowHandle;
            identifier.guidItem = Id;

            // Get RECT
            PInvoke.Shell_NotifyIconGetRect(in identifier, out var _IconLocation);

            return new Rect(
                _IconLocation.left,
                _IconLocation.top,
                _IconLocation.right - _IconLocation.left,
                _IconLocation.bottom - _IconLocation.top);
        }
    }

    public event RoutedEventHandler? LeftClick;

    public event RoutedEventHandler? RightClick;

    // Constructor

    /// <summary>
    /// Initializes an instance of <see cref="SystemTrayIcon"/>.
    /// </summary>
    /// <remarks>
    /// Note that initializing an instance won't make the icon visible.
    /// </remarks>
    public SystemTrayIcon()
    {
        _taskbarRestartMessageId = PInvoke.RegisterWindowMessage("TaskbarCreated");

        Id = _trayIconGuid;
        _IconWindow = new SystemTrayIconWindow(this);

        CreateOrModifyNotifyIcon();
    }

    // Public Methods

    /// <summary>
    /// Shows the tray icon.
    /// </summary>
    public SystemTrayIcon Show()
    {
        IsVisible = true;

        return this;
    }

    /// <summary>
    /// Hides the tray icon.
    /// </summary>
    public SystemTrayIcon Hide()
    {
        IsVisible = false;

        return this;
    }

    // Private Methods

    private void CreateOrModifyNotifyIcon()
    {
        if (IsVisible)
        {
            NOTIFYICONDATAW lpData = default;

            lpData.cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW));
            lpData.hWnd = _IconWindow.WindowHandle;
            lpData.uCallbackMessage = WM_FILES_UNIQUE_MESSAGE;
            lpData.hIcon = (Icon != null) ? new HICON(Icon.Handle) : default;
            lpData.guidItem = Id;
            lpData.uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP;
            lpData.szTip = _Tooltip ?? string.Empty;

            if (!_notifyIconCreated)
            {
                // Delete the existing icon
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in lpData);

                _notifyIconCreated = true;

                // Add a new icon
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in lpData);

                lpData.Anonymous.uVersion = 4u;

                // Set the icon handler version
                // NOTE: Do not omit this code. If you remove, the icon won't be shown.
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_SETVERSION, in lpData);
            }
            else
            {
                // Modify the existing icon
                PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, in lpData);
            }
        }
    }

    private void DeleteNotifyIcon()
    {
        if (_notifyIconCreated)
        {
            _notifyIconCreated = false;

            NOTIFYICONDATAW lpData = default;

            lpData.cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW));
            lpData.hWnd = _IconWindow.WindowHandle;
            lpData.guidItem = Id;
            lpData.uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP;

            // Delete the existing icon
            PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in lpData);
        }
    }

    internal LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case WM_FILES_UNIQUE_MESSAGE:
                {
                    switch ((uint)(lParam.Value & 0xFFFF))
                    {
                        case PInvoke.WM_LBUTTONUP:
                            {
                                App.API.LogVerbose(ClassName, "Left clicked");
                                LeftClick?.Invoke(this, new RoutedEventArgs());
                                break;
                            }
                        case PInvoke.WM_RBUTTONUP:
                            {
                                App.API.LogVerbose(ClassName, "Right clicked");
                                RightClick?.Invoke(this, new RoutedEventArgs());
                                break;
                            }
                    }

                    break;
                }
            case PInvoke.WM_DESTROY:
                {
                    App.API.LogVerbose(ClassName, "Window destroyed");
                    DeleteNotifyIcon();
                    break;
                }
            default:
                {
                    if (uMsg == _taskbarRestartMessageId)
                    {
                        App.API.LogVerbose(ClassName, "Taskbar restarted");
                        DeleteNotifyIcon();
                        CreateOrModifyNotifyIcon();
                    }

                    return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
                }
        }
        return default;
    }

    public void Dispose()
    {
        _IconWindow.Dispose();
    }
}
