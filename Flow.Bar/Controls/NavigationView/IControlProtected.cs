using System.Windows;

namespace Flow.Bar.Controls;

internal interface IControlProtected
{
    DependencyObject GetTemplateChild(string childName);
}
