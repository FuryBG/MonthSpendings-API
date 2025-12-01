using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SpendingRelationToBudgetPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetPeriodId",
                table: "Spendings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Spendings_BudgetPeriodId",
                table: "Spendings",
                column: "BudgetPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Spendings_BudgetPeriods_BudgetPeriodId",
                table: "Spendings",
                column: "BudgetPeriodId",
                principalTable: "BudgetPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spendings_BudgetPeriods_BudgetPeriodId",
                table: "Spendings");

            migrationBuilder.DropIndex(
                name: "IX_Spendings_BudgetPeriodId",
                table: "Spendings");

            migrationBuilder.DropColumn(
                name: "BudgetPeriodId",
                table: "Spendings");
        }
    }
}
