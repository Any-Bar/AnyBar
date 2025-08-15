using System.Windows;
using iNKORE.UI.WPF.Helpers;
using iNKORE.UI.WPF.Modern.Controls;

namespace Flow.Bar.Controls;

public class StackViewItem : StackViewBaseItem
{
    static StackViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackViewItem), new FrameworkPropertyMetadata(typeof(StackViewItem)));
    }

    public StackViewItem()
    {
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ContentPresenterEx = this.GetTemplateChild<ContentPresenterEx>("ContentPresenter");
    }

    internal ContentPresenterEx ContentPresenterEx { get; private set; } = null!;
}
