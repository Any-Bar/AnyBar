using System.Windows;
using System.Windows.Controls;

namespace Flow.Bar.Controls;


/// <summary>
/// <see cref="StyleSelector"/> used by <see cref="SettingsExpanderEx"/> to choose the proper <see cref="SettingsCardEx"/> container style (clickable or not).
/// </summary>
public class SettingsExpanderExItemStyleSelector : StyleSelector
{
    /// <summary>
    /// Gets or sets the default <see cref="Style"/>.
    /// </summary>
    public Style DefaultStyle { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Style"/> when clickable.
    /// </summary>
    public Style ClickableStyle { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsExpanderExItemStyleSelector"/> class.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SettingsExpanderExItemStyleSelector()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    /// <inheritdoc/>
    public override Style SelectStyle(object item, DependencyObject container)
    {
        if (container is SettingsCardEx card && card.IsClickEnabled)
        {
            return ClickableStyle;
        }
        else
        {
            return DefaultStyle;
        }
    }
}
