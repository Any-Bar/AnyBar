namespace Flow.Bar.Plugin;

/// <summary>
/// Enumeration for popup modes of context menus.
/// </summary>
public enum ContextMenuPopupMode
{
    /// <summary>
    /// Context menu will popup on each click and fade away after losing focus.
    /// </summary>
    AlwaysPopup,

    /// <summary>
    /// Context menu will popup on the first click and fade away on the second click or after losing focus.
    /// </summary>
    PopupAndFadeAway
}
