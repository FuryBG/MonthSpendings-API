namespace Application.Localization
{
    /// <summary>
    /// The languages the app is translated into. Matching a request to one of these (incl. "bg-BG" → "bg"
    /// and the English fallback) is done by ASP.NET Core's RequestLocalization middleware, not here.
    /// </summary>
    public static class SupportedLanguages
    {
        public const string English = "en";
        public const string Bulgarian = "bg";
        public const string Spanish = "es";
        public const string Default = English;

        public static readonly string[] All = [English, Bulgarian, Spanish];
    }
}
