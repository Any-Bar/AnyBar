using System.Collections.Generic;

namespace Flow.Bar.Models.Language;

internal static class AvailableLanguages
{
    public static Language English = new("en", "English");

    public static List<Language> GetAvailableLanguages()
    {
        return
        [
            English,
        ];
    }

    public static string GetSystemTranslation(string languageCode)
    {
        return languageCode switch
        {
            _ => "System"
        };
    }
}
