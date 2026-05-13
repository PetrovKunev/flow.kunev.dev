using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowKunevDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyBudgetFilterPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DailyBudgetAccountIds",
                table: "AspNetUsers",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DailyBudgetFromDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DailyBudgetToDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyBudgetAccountIds",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DailyBudgetFromDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DailyBudgetToDate",
                table: "AspNetUsers");
        }
    }
}
