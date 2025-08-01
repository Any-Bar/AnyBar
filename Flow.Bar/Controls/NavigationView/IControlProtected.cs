using System.Windows;

namespace Flow.Bar.Controls.NavigationView;

internal interface IControlProtected
{
    DependencyObject GetTemplateChild(string childName);
}
