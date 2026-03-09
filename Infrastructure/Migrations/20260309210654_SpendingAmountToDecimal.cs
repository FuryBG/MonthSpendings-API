using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SpendingAmountToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_SpendingId",
                table: "BankTransactions");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Spendings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<int>(
                name: "BankTransactionId",
                table: "Spendings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_SpendingId",
                table: "BankTransactions",
                column: "SpendingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_SpendingId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "BankTransactionId",
                table: "Spendings");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Spendings",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_SpendingId",
                table: "BankTransactions",
                column: "SpendingId");
        }
    }
}
