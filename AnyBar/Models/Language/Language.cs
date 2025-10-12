namespace AnyBar.Models.Language;

public class Language(string code, string display)
{
    /// <summary>
    /// E.g. En or Zh-CN
    /// </summary>
    public string LanguageCode { get; set; } = code;

    public string Display { get; set; } = display;
}
