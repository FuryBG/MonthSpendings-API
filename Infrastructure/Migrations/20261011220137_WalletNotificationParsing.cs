using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WalletNotificationParsing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardLast4",
                table: "NotificationTransactions",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "NotificationTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ParseFailed",
                table: "NotificationTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardLast4",
                table: "NotificationTransactions");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "NotificationTransactions");

            migrationBuilder.DropColumn(
                name: "ParseFailed",
                table: "NotificationTransactions");
        }
    }
}
