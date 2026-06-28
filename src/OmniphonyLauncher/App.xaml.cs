using System.Windows;

namespace OmniphonyLauncher;
public partial class App : Application
{
    public static void SetLanguage(string language)
    {
        var code = language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en-US" : "zh-CN";
        var dictionaries = Current.Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(x => x.Source?.OriginalString.Contains("Strings.", StringComparison.OrdinalIgnoreCase) == true);
        if (current is not null) dictionaries.Remove(current);
        dictionaries.Insert(0, new ResourceDictionary { Source = new Uri($"Resources/Strings.{code}.xaml", UriKind.Relative) });
    }
}
