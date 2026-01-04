using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Currency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Budgets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "Name", "Symbol" },
                values: new object[,]
                {
                    { 1, "USD", "US Dollar", "$" },
                    { 2, "EUR", "Euro", "€" },
                    { 3, "GBP", "British Pound", "£" },
                    { 4, "JPY", "Japanese Yen", "¥" },
                    { 5, "CAD", "Canadian Dollar", "C$" },
                    { 6, "AUD", "Australian Dollar", "A$" },
                    { 7, "CHF", "Swiss Franc", "CHF" },
                    { 8, "CNY", "Chinese Yuan Renminbi", "¥" },
                    { 9, "SEK", "Swedish Krona", "kr" },
                    { 10, "NOK", "Norwegian Krone", "kr" },
                    { 11, "DKK", "Danish Krone", "kr" },
                    { 12, "NZD", "New Zealand Dollar", "NZ$" },
                    { 13, "SGD", "Singapore Dollar", "S$" },
                    { 14, "HKD", "Hong Kong Dollar", "HK$" },
                    { 15, "KRW", "South Korean Won", "₩" },
                    { 16, "INR", "Indian Rupee", "₹" },
                    { 17, "MXN", "Mexican Peso", "$" },
                    { 18, "BRL", "Brazilian Real", "R$" },
                    { 19, "RUB", "Russian Ruble", "₽" },
                    { 20, "ZAR", "South African Rand", "R" },
                    { 21, "TRY", "Turkish Lira", "₺" },
                    { 22, "AED", "UAE Dirham", "د.إ" },
                    { 23, "PLN", "Polish Zloty", "zł" },
                    { 24, "THB", "Thai Baht", "฿" },
                    { 25, "IDR", "Indonesian Rupiah", "Rp" },
                    { 26, "MYR", "Malaysian Ringgit", "RM" },
                    { 27, "PHP", "Philippine Peso", "₱" },
                    { 28, "HUF", "Hungarian Forint", "Ft" },
                    { 29, "CZK", "Czech Koruna", "Kč" },
                    { 30, "ILS", "Israeli Shekel", "₪" },
                    { 31, "CLP", "Chilean Peso", "$" },
                    { 32, "PKR", "Pakistani Rupee", "₨" },
                    { 33, "EGP", "Egyptian Pound", "£" },
                    { 34, "SAR", "Saudi Riyal", "﷼" },
                    { 35, "COP", "Colombian Peso", "$" },
                    { 36, "VND", "Vietnamese Dong", "₫" },
                    { 37, "BDT", "Bangladeshi Taka", "৳" },
                    { 38, "NGN", "Nigerian Naira", "₦" },
                    { 39, "KWD", "Kuwaiti Dinar", "د.ك" },
                    { 40, "QAR", "Qatari Riyal", "﷼" },
                    { 41, "OMR", "Omani Rial", "﷼" },
                    { 42, "TWD", "New Taiwan Dollar", "NT$" },
                    { 43, "ARS", "Argentine Peso", "$" },
                    { 44, "UAH", "Ukrainian Hryvnia", "₴" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CurrencyId",
                table: "Budgets",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Currencies_CurrencyId",
                table: "Budgets",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Currencies_CurrencyId",
                table: "Budgets");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_CurrencyId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Budgets");
        }
    }
}
