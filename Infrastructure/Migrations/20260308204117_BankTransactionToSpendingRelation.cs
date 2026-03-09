using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankTransactionToSpendingRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_BudgetCategories_BudgetCategoryId",
                table: "BankTransactions");

            migrationBuilder.RenameColumn(
                name: "BudgetCategoryId",
                table: "BankTransactions",
                newName: "SpendingId");

            migrationBuilder.RenameIndex(
                name: "IX_BankTransactions_BudgetCategoryId",
                table: "BankTransactions",
                newName: "IX_BankTransactions_SpendingId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Spendings_SpendingId",
                table: "BankTransactions",
                column: "SpendingId",
                principalTable: "Spendings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Spendings_SpendingId",
                table: "BankTransactions");

            migrationBuilder.RenameColumn(
                name: "SpendingId",
                table: "BankTransactions",
                newName: "BudgetCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_BankTransactions_SpendingId",
                table: "BankTransactions",
                newName: "IX_BankTransactions_BudgetCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_BudgetCategories_BudgetCategoryId",
                table: "BankTransactions",
                column: "BudgetCategoryId",
                principalTable: "BudgetCategories",
                principalColumn: "Id");
        }
    }
}
