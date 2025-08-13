using System.Windows;

namespace Flow.Bar.Controls;

internal static class CppWinRTHelpers
{
    public static WinRTReturn? GetTemplateChildT<WinRTReturn>(string childName, IControlProtected controlProtected) where WinRTReturn : DependencyObject
    {
        var childAsDO = controlProtected.GetTemplateChild(childName);

        if (childAsDO != null)
        {
            return childAsDO as WinRTReturn;
        }
        return null;
    }
}
