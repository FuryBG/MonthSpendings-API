using System.Globalization;
using Application.Services.NotificationParsing;

namespace Application.Tests
{
    /// <summary>
    /// Fixture table of real-world notification shapes. When a "ParseFailed" row shows up in the DB,
    /// add its RawTitle/RawBody here with the expected result, then fix the parser.
    /// </summary>
    public class WalletNotificationParserTests
    {
        private readonly WalletNotificationParser _parser = new();

        // title, text, currency hint, expected amount, expected currency, expected merchant, expected card last 4
        public static TheoryData<string, string, string?, string, string, string, string?> Parsed => new()
        {
            // English, symbol before
            { "Lidl", "€12.34 with Visa •••• 1234", "EUR", "12.34", "EUR", "Lidl", "1234" },
            { "Starbucks", "$5.50 with Mastercard **** 9876", "USD", "5.50", "USD", "Starbucks", "9876" },
            { "Tesco", "£1,234.56 with Visa ** 4321", null, "1234.56", "GBP", "Tesco", "4321" },
            { "Lidl", "€5,50", "EUR", "5.50", "EUR", "Lidl", null },
            // Bulgarian, lev text after
            { "Билла", "12,34 лв. с Visa ••1234", "BGN", "12.34", "BGN", "Билла", "1234" },
            { "Kaufland", "1 234,56 лв.", "BGN", "1234.56", "BGN", "Kaufland", null },
            { "Kaufland", "лв 5.50", "BGN", "5.50", "BGN", "Kaufland", null },
            { "OMV", "Плащане 45,00 BGN с карта ** 1111", "BGN", "45.00", "BGN", "OMV", "1111" },
            // Spanish, symbol after
            { "Mercadona", "12,34 € con Visa •••• 5678", "EUR", "12.34", "EUR", "Mercadona", "5678" },
            { "El Corte Inglés", "1.234,50 € con Mastercard", "EUR", "1234.50", "EUR", "El Corte Inglés", null },
            // NBSP / narrow NBSP separators
            { "Carrefour", "1 234,56 €", "EUR", "1234.56", "EUR", "Carrefour", null },
            { "Carrefour", "12,34 €", "EUR", "12.34", "EUR", "Carrefour", null },
            // ISO code before / after, glued
            { "Amazon", "EUR 99.99", "BGN", "99.99", "EUR", "Amazon", null },
            { "Amazon", "99.99EUR", "BGN", "99.99", "EUR", "Amazon", null },
            { "Uber", "Paid USD 7 with card", "EUR", "7", "USD", "Uber", null },
            // Other currencies
            { "Żabka", "23,99 zł", "PLN", "23.99", "PLN", "Żabka", null },
            { "ICA", "149,00 kr", "SEK", "149.00", "SEK", "ICA", null },
            { "Netto", "149,00 kr.", "DKK", "149.00", "DKK", "Netto", null },
            { "Spar", "2 500 Ft", "HUF", "2500", "HUF", "Spar", null },
            { "Albert", "349,90 Kč", "CZK", "349.90", "CZK", "Albert", null },
            { "Migros", "₺150,00", "TRY", "150.00", "TRY", "Migros", null },
            { "Carrefour", "R$ 25,90", "BRL", "25.90", "BRL", "Carrefour", null },
            { "Talabat", "12.500 KWD", "KWD", "12.500", "KWD", "Talabat", null },
            // Ambiguous symbol resolved by the hint
            { "Tim Hortons", "$4.25", "CAD", "4.25", "CAD", "Tim Hortons", null },
            { "Rema 1000", "kr 89,90", "NOK", "89.90", "NOK", "Rema 1000", null },
            // Number in merchant name must not be read as the amount
            { "Shell 1234", "€5.00 with Visa •••• 1111", "EUR", "5.00", "EUR", "Shell 1234", "1111" },
            { "7-Eleven", "$3.10", "USD", "3.10", "USD", "7-Eleven", null },
            // Balance line after the payment is ignored
            { "DSK", "Плащане 20,00 лв. Наличност: 1 500,00 лв.", "BGN", "20.00", "BGN", "DSK", null },
            // Title is the card / app, merchant is in the body
            { "Visa •••• 1234", "€8.40 at Costa Coffee", "EUR", "8.40", "EUR", "at Costa Coffee", "1234" },
            { "Google Wallet", "Lidl €3.20", "EUR", "3.20", "EUR", "Lidl", null },
            // Date / time in the text is not an amount
            { "Lidl", "12.10.2026 18:45 €7.30", "EUR", "7.30", "EUR", "Lidl", null },
            // No currency at all → user's currency
            { "Lidl", "Payment 7.30 with Visa •••• 1234", "BGN", "7.30", "BGN", "Lidl", "1234" },
            // Bidi marks around the amount
            { "Lidl", "‎€‎12.34", "EUR", "12.34", "EUR", "Lidl", null },
        };

        [Theory]
        [MemberData(nameof(Parsed))]
        public void Parses_amount_currency_merchant_and_card(
            string title, string text, string? hint, string amount, string currency, string merchant, string? last4)
        {
            WalletNotificationParseResult result = _parser.Parse(title, text, hint);

            Assert.True(result.Success, result.FailureReason);
            Assert.Equal(decimal.Parse(amount, CultureInfo.InvariantCulture), result.Amount);
            Assert.Equal(currency, result.Currency);
            Assert.Equal(merchant, result.Merchant);
            Assert.Equal(last4, result.CardLast4);
        }

        [Theory]
        [InlineData("Google Wallet", "Card added to Google Wallet", null)]
        [InlineData("Lidl", "Your card is ready to use", "EUR")]
        [InlineData("Lidl", "Paid 7.30 and 2.10", "EUR")]
        [InlineData("Lidl", "Payment 7.30", null)]
        public void Fails_without_a_readable_amount(string title, string text, string? hint)
        {
            WalletNotificationParseResult result = _parser.Parse(title, text, hint);

            Assert.False(result.Success);
            Assert.Equal(0, result.Amount);
            Assert.NotNull(result.FailureReason);
        }

        [Theory]
        [InlineData("**** 1234", "1234")]
        [InlineData("** 1234", "1234")]
        [InlineData("•••• 5678", "5678")]
        [InlineData("••5678", "5678")]
        public void Keeps_card_last4_even_when_parsing_fails(string card, string last4)
        {
            WalletNotificationParseResult result = _parser.Parse("Lidl", $"Card {card} added", null);

            Assert.False(result.Success);
            Assert.Equal(last4, result.CardLast4);
        }
    }
}
