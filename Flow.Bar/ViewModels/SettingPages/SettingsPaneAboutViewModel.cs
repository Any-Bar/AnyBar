using CommunityToolkit.Mvvm.ComponentModel;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneAboutViewModel : ObservableObject
{
    private static readonly string ClassName = nameof(SettingsPaneAboutViewModel);

    public string Version => Constants.Version switch
    {
        "1.0.0" => Constants.Dev,
        _ => Constants.Version
    };
}
