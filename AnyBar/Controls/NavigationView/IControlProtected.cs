using System.Windows;

namespace AnyBar.Controls;

internal interface IControlProtected
{
    DependencyObject GetTemplateChild(string childName);
}
