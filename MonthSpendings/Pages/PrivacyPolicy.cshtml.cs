using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace MonthSpendings.Pages
{
    public class PrivacyPolicyModel : PageModel
    {
        public string Language { get; private set; } = "en";
        public PrivacyPolicyContent Policy { get; private set; } = PrivacyPolicyContent.For("en");

        public void OnGet()
        {
            // Resolved by RequestLocalization from ?lang= or Accept-Language (always en/bg/es).
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            Policy = PrivacyPolicyContent.For(Language);
        }
    }
}
