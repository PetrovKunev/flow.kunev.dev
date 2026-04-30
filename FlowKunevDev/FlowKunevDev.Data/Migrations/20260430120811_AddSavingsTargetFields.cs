using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowKunevDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingsTargetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedMonthlyIncome",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySavingsPercent",
                table: "AspNetUsers",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalaryAccountId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SavingsAccountId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedMonthlyIncome",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MonthlySavingsPercent",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalaryAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SavingsAccountId",
                table: "AspNetUsers");
        }
    }
}
