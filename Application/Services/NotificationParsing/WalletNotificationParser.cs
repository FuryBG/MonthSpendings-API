using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services.NotificationParsing
{
    public sealed record WalletNotificationParseResult(
        bool Success,
        decimal Amount,
        string Currency,
        string Merchant,
        string? CardLast4,
        string? FailureReason)
    {
        public static WalletNotificationParseResult Parsed(decimal amount, string currency, string merchant, string? cardLast4) =>
            new(true, amount, currency, merchant, cardLast4, null);

        public static WalletNotificationParseResult Failed(string reason, string merchant, string? cardLast4) =>
            new(false, 0, "", merchant, cardLast4, reason);
    }

    public interface IWalletNotificationParser
    {
        /// <param name="currencyHint">ISO code used to resolve ambiguous symbols ($, kr, ¥) and amounts without a currency.</param>
        WalletNotificationParseResult Parse(string? title, string? text, string? currencyHint);
    }

    /// <summary>
    /// Language-independent parser for payment notifications. Finds a number with a currency token on
    /// either side (€5.50, 5,50 €, 1 234,56 лв., EUR 5.50, 5.50 BGN), regardless of word order.
    /// </summary>
    public class WalletNotificationParser : IWalletNotificationParser
    {
        // Alias → candidate ISO codes; the first one is the default when the hint doesn't pick another.
        private static readonly Dictionary<string, string[]> Aliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["€"] = ["EUR"],
            ["$"] = ["USD", "CAD", "AUD", "NZD", "SGD", "HKD", "MXN", "CLP", "COP", "ARS", "TWD"],
            ["US$"] = ["USD"],
            ["C$"] = ["CAD"], ["CA$"] = ["CAD"],
            ["A$"] = ["AUD"], ["AU$"] = ["AUD"],
            ["NZ$"] = ["NZD"], ["S$"] = ["SGD"], ["HK$"] = ["HKD"], ["NT$"] = ["TWD"],
            ["MX$"] = ["MXN"], ["R$"] = ["BRL"],
            ["£"] = ["GBP", "EGP"],
            ["¥"] = ["JPY", "CNY"], ["￥"] = ["JPY", "CNY"], ["円"] = ["JPY"], ["元"] = ["CNY"],
            ["₩"] = ["KRW"], ["₹"] = ["INR"], ["₨"] = ["PKR", "INR"], ["Rs"] = ["INR", "PKR"], ["Rs."] = ["INR", "PKR"],
            ["₽"] = ["RUB"], ["руб"] = ["RUB"], ["руб."] = ["RUB"],
            ["₺"] = ["TRY"], ["TL"] = ["TRY"],
            ["₴"] = ["UAH"], ["грн"] = ["UAH"], ["грн."] = ["UAH"],
            ["₾"] = ["GEL"], ["₪"] = ["ILS"], ["₫"] = ["VND"], ["₱"] = ["PHP"], ["฿"] = ["THB"],
            ["৳"] = ["BDT"], ["₦"] = ["NGN"], ["﷼"] = ["SAR", "QAR", "OMR"],
            ["د.إ"] = ["AED"], ["د.ك"] = ["KWD"],
            ["лв"] = ["BGN"], ["лв."] = ["BGN"],
            ["zł"] = ["PLN"], ["Kč"] = ["CZK"], ["Ft"] = ["HUF"], ["lei"] = ["RON"],
            ["kr"] = ["SEK", "NOK", "DKK", "ISK"], ["kr."] = ["DKK", "SEK", "NOK", "ISK"],
            ["Fr."] = ["CHF"], ["ден"] = ["MKD"], ["дин"] = ["RSD"], ["din"] = ["RSD"], ["KM"] = ["BAM"],
            ["Rp"] = ["IDR"], ["RM"] = ["MYR"], ["R"] = ["ZAR"],
        };

        private static readonly HashSet<string> IsoCodes =
        [
            "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "SEK", "NOK", "DKK", "NZD", "SGD", "HKD",
            "KRW", "INR", "MXN", "BRL", "RUB", "ZAR", "TRY", "AED", "PLN", "THB", "IDR", "MYR", "PHP", "HUF",
            "CZK", "ILS", "CLP", "PKR", "EGP", "SAR", "COP", "VND", "BDT", "NGN", "KWD", "QAR", "OMR", "TWD",
            "ARS", "UAH", "BGN", "RON", "RSD", "GEL", "ISK", "MKD", "BAM", "MDL", "ALL", "AMD", "AZN", "BYN",
            "KZT", "BHD", "JOD", "MAD", "TND", "PEN", "UYU", "KES",
        ];

        // Currencies with 3 minor digits: "12.345 KWD" is 12.345, not 12345.
        private static readonly HashSet<string> ThreeDecimalCurrencies = ["KWD", "BHD", "OMR", "JOD", "TND"];

        private static readonly string[] BalanceKeywords =
            ["balance", "available", "avail", "наличност", "разполагаем", "салдо", "saldo", "disponible"];

        private static readonly string[] AppTitles =
            ["google wallet", "google pay", "wallet", "gpay", "android pay"];

        // "**** 1234", "** 1234", "•• 1234", "••1234"
        private static readonly Regex CardRegex = new(@"[•*·●∙]+\s?(?<last4>\d{4})(?!\d)", RegexOptions.Compiled);
        private static readonly Regex DateRegex = new(@"(?<!\d)\d{1,4}[./-]\d{1,2}[./-]\d{1,4}(?!\d)", RegexOptions.Compiled);
        private static readonly Regex TimeRegex = new(@"(?<!\d)\d{1,2}:\d{2}(?::\d{2})?(?!\d)", RegexOptions.Compiled);
        private static readonly Regex BareDecimalRegex = new(@"(?<![\d.,])\d+[.,]\d{2}(?![\d.,])", RegexOptions.Compiled);
        private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        private static readonly Regex MoneyRegex = BuildMoneyRegex();

        private static Regex BuildMoneyRegex()
        {
            static string Alternation(IEnumerable<string> tokens) =>
                string.Join("|", tokens.OrderByDescending(t => t.Length).Select(Regex.Escape));

            // Letter aliases match in any case (лв / ЛВ); ISO codes only in upper case so "try"/"all" in prose never match.
            var letterAliases = Aliases.Keys.Where(k => k.Any(char.IsLetter));
            var symbolAliases = Aliases.Keys.Where(k => !k.Any(char.IsLetter));
            string currency = $@"(?<![\p{{L}}])(?:(?i:{Alternation(letterAliases)})|{Alternation(symbolAliases)}|{Alternation(IsoCodes)})(?![\p{{L}}])";

            // 1 234,56 / 1.234,56 / 1,234.56 / 1'234.56 / 1234,56 / 12.5 / 12
            const string number = @"\d{1,3}(?:[ .,'’]\d{3})+(?:[.,]\d{1,3})?|\d+(?:[.,]\d{1,3})?";

            // A trailing currency that is itself followed by a number belongs to that number ("Shell 1234 €5.00").
            string pattern = $@"(?:(?<c1>{currency})\s?[-+]?\s?)?(?<![\d.,])(?<num>{number})(?!\d)(?:\s?(?<c2>{currency})(?!\s?[-+]?\d))?";
            return new Regex(pattern, RegexOptions.Compiled);
        }

        public WalletNotificationParseResult Parse(string? title, string? text, string? currencyHint)
        {
            string cleanTitle = Normalize(title);
            string cleanText = Normalize(text);

            string? cardLast4 = CardRegex.Match(cleanTitle + " " + cleanText) is { Success: true } card
                ? card.Groups["last4"].Value
                : null;

            // Card digits, dates and times are masked so they are never read as an amount.
            string searchable = Mask(cleanText) + " | " + Mask(cleanTitle);
            string fallbackMerchant = cleanTitle;

            foreach (Match match in MoneyRegex.Matches(searchable))
            {
                Group currencyGroup = match.Groups["c1"].Success ? match.Groups["c1"] : match.Groups["c2"];
                if (!currencyGroup.Success)
                    continue;
                if (FollowsBalanceKeyword(searchable, match.Index))
                    continue;

                string currency = ResolveCurrency(currencyGroup.Value, currencyHint);
                decimal? amount = ParseNumber(match.Groups["num"].Value, currency);
                if (amount is not > 0)
                    continue;

                string merchant = ResolveMerchant(cleanTitle, cleanText, match.Value);
                return WalletNotificationParseResult.Parsed(amount.Value, currency, merchant, cardLast4);
            }

            // No currency next to any number: accept a single "12.34"-style amount in the user's currency.
            if (!string.IsNullOrEmpty(currencyHint))
            {
                var bare = BareDecimalRegex.Matches(Mask(cleanText))
                    .Where(m => !FollowsBalanceKeyword(Mask(cleanText), m.Index))
                    .ToList();
                if (bare.Count == 1 && ParseNumber(bare[0].Value, currencyHint) is > 0 and decimal bareAmount)
                {
                    string merchant = ResolveMerchant(cleanTitle, cleanText, bare[0].Value);
                    return WalletNotificationParseResult.Parsed(bareAmount, currencyHint, merchant, cardLast4);
                }
            }

            return WalletNotificationParseResult.Failed("no amount with a currency found", fallbackMerchant, cardLast4);
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            var sb = new StringBuilder(value.Length);
            foreach (char c in value.Normalize(NormalizationForm.FormC))
            {
                switch (c)
                {
                    // NBSP, narrow NBSP, thin/figure spaces → space
                    case ' ' or ' ' or ' ' or ' ' or ' ' or ' ':
                        sb.Append(' ');
                        break;
                    // bidi marks and zero-width characters
                    case '‎' or '‏' or '​' or '‌' or '‍' or '⁦' or '⁧' or '⁨' or '⁩' or '‪' or '‫' or '‬' or '﻿':
                        break;
                    // unicode minus / dashes → hyphen
                    case '−' or '–' or '—':
                        sb.Append('-');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return WhitespaceRegex.Replace(sb.ToString(), " ").Trim();
        }

        private static string Mask(string value)
        {
            value = CardRegex.Replace(value, m => new string(' ', m.Length));
            value = DateRegex.Replace(value, m => new string(' ', m.Length));
            return TimeRegex.Replace(value, m => new string(' ', m.Length));
        }

        private static bool FollowsBalanceKeyword(string text, int index)
        {
            int start = Math.Max(0, index - 30);
            string before = text[start..index];
            return BalanceKeywords.Any(k => before.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private static string ResolveCurrency(string token, string? hint)
        {
            if (IsoCodes.Contains(token))
                return token;

            string[] candidates = Aliases.TryGetValue(token, out var codes) ? codes : [token.ToUpperInvariant()];
            return hint != null && candidates.Contains(hint, StringComparer.OrdinalIgnoreCase)
                ? hint.ToUpperInvariant()
                : candidates[0];
        }

        /// <summary>
        /// The last '.' or ',' followed by 1–2 digits is the decimal separator; every other separator groups thousands.
        /// Exactly 3 digits after the last separator means thousands, unless the currency has 3 minor digits.
        /// </summary>
        internal static decimal? ParseNumber(string raw, string currency)
        {
            int lastSep = raw.LastIndexOfAny(['.', ',', ' ', '\'', '’']);
            if (lastSep < 0)
                return decimal.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var whole) ? whole : null;

            string fraction = raw[(lastSep + 1)..];
            char sep = raw[lastSep];
            bool singleSeparator = raw.IndexOfAny(['.', ',', ' ', '\'', '’']) == lastSep;
            bool isDecimal = (sep is '.' or ',') &&
                (fraction.Length <= 2 || (fraction.Length == 3 && singleSeparator && ThreeDecimalCurrencies.Contains(currency)));

            string integerPart = new(raw[..lastSep].Where(char.IsDigit).ToArray());
            string normalized = isDecimal ? $"{integerPart}.{fraction}" : integerPart + fraction;

            return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
        }

        private static string ResolveMerchant(string title, string text, string moneyText)
        {
            bool titleUsable = title.Length > 0
                && !CardRegex.IsMatch(title)
                && !title.Contains(moneyText, StringComparison.Ordinal)
                && !AppTitles.Contains(title.ToLowerInvariant());
            if (titleUsable)
                return title;

            // Title is a card / app name / the amount itself: take what's left of the body.
            string remainder = CardRegex.Replace(text.Replace(moneyText, " "), " ");
            remainder = WhitespaceRegex.Replace(remainder, " ").Trim(' ', '-', '·', '•', ',', ':', '|', '.');
            return remainder.Length > 0 ? remainder : title;
        }
    }
}
