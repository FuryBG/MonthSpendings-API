using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScopeTransactionIdByAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""BankTransactions"" SET ""TransactionId"" = ""BankAccountId""::text || '/' || ""TransactionId"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""BankTransactions"" SET ""TransactionId"" = substring(""TransactionId"" from position('/' in ""TransactionId"") + 1);");
        }
    }
}
