namespace Chainbot.Contracts.Classes
{
    public static class GlobalConfig
    {
        public enum enTheme
        {
            Light,
            Dark
        }

        public static readonly enTheme DefaultTheme = enTheme.Light;

        public static enTheme CurrentTheme { get; set; } = DefaultTheme;

        public enum enLanguage
        {
            English,
            Chinese
        }

        public static readonly enLanguage DefaultLanguage = enLanguage.English;

        public static enLanguage CurrentLanguage { get; set; } = DefaultLanguage;

    }
}
