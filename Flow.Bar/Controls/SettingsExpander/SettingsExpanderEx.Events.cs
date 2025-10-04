using System;

namespace Flow.Bar.Controls;

public partial class SettingsExpanderEx
{
    /// <summary>
    /// Fires when the SettingsExpanderEx is opened
    /// </summary>
    public event EventHandler? Expanded;

    /// <summary>
    /// Fires when the expander is closed
    /// </summary>
    public event EventHandler? Collapsed;
}
