using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SpendingCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: add as nullable so existing rows don't fail the constraint
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Spendings",
                type: "integer",
                nullable: true);

            // Step 2: backfill existing rows — assign each spending to any member of its budget
            migrationBuilder.Sql("""
                UPDATE "Spendings" s
                SET "CreatedByUserId" = (
                    SELECT aub."UsersId"
                    FROM "AppUserBudget" aub
                    JOIN "BudgetCategories" bc ON bc."BudgetId" = aub."BudgetsId"
                    WHERE bc."Id" = s."BudgetCategoryId"
                    LIMIT 1
                )
                WHERE s."CreatedByUserId" IS NULL;
                """);

            // Step 3: enforce NOT NULL now that every row has a value
            migrationBuilder.AlterColumn<int>(
                name: "CreatedByUserId",
                table: "Spendings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spendings_CreatedByUserId",
                table: "Spendings",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Spendings_Users_CreatedByUserId",
                table: "Spendings",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spendings_Users_CreatedByUserId",
                table: "Spendings");

            migrationBuilder.DropIndex(
                name: "IX_Spendings_CreatedByUserId",
                table: "Spendings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Spendings");
        }
    }
}
