using System;
using System.Linq;
using System.Windows;

namespace KingVoiceChanger
{
    public partial class App : Application
    {
        public enum Theme { Light, Dark }

        public Theme CurrentTheme { get; private set; } = Theme.Dark;

        public void ChangeTheme(Theme theme)
        {
            CurrentTheme = theme;

            var existingTheme = Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("LightTheme") || d.Source.OriginalString.Contains("DarkTheme")));
            if (existingTheme != null)
            {
                Resources.MergedDictionaries.Remove(existingTheme);
            }

            var themeUri = theme == Theme.Dark ? "DarkTheme.xaml" : "LightTheme.xaml";
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) });
        }
    }
}
