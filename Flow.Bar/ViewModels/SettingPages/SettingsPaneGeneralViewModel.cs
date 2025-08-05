using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Bar.Models.Language;
using Flow.Bar.Models.UserSettings;
using System.Collections.Generic;

namespace Flow.Bar.ViewModels.SettingPages;

public partial class SettingsPaneGeneralViewModel(Settings settings, Internationalization translater) : ObservableObject
{
    public Settings Settings { get; init; } = settings;

    private readonly Internationalization _translater = translater;

    public List<Language> Languages => _translater.LoadAvailableLanguages();

    public string Language
    {
        get => Settings.Language;
        set => _translater.ChangeLanguage(value);
    }
}
