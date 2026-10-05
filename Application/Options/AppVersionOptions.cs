namespace Application.Options
{
    public class AppVersionOptions
    {
        /// <summary>Newest native version published on the stores. Older installs get a dismissible update prompt.</summary>
        public string LatestVersion { get; set; } = "1.0.0";

        /// <summary>Oldest native version still allowed to run. Older installs get a blocking update prompt.</summary>
        public string MinimumVersion { get; set; } = "1.0.0";

        public string AndroidStoreUrl { get; set; } = "https://play.google.com/store/apps/details?id=app.expo.tavira";
    }
}
