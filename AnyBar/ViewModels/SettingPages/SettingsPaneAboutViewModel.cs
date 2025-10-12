using System;
using AnyBar.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnyBar.ViewModels;

public partial class SettingsPaneAboutViewModel : ObservableObject, INavigationHeader
{
    #region Version

    public string Version => Constants.Version switch
    {
        "1.0.0" => Constants.Dev,
        _ => Constants.Version
    };

    #endregion

    #region INavigationHeader

    public string? GetHeaderKey()
    {
        return nameof(Localize.SettingWindow_About);
    }

    public string GetHeaderValue()
    {
        throw new NotImplementedException();
    }

    #endregion
}
