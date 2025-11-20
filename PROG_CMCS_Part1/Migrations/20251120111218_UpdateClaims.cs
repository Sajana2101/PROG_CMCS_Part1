using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG_CMCS_Part1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoordinatorComment",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoordinatorId",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateApproved",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateVerified",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerComment",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoordinatorComment",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "CoordinatorId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DateApproved",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DateVerified",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ManagerComment",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Claims");
        }
    }
}
