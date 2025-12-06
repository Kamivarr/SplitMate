using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurePaidByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_PaidById",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "PaidById",
                table: "Expenses",
                newName: "PaidByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_PaidById",
                table: "Expenses",
                newName: "IX_Expenses_PaidByUserId");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "ExpenseShares",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Expenses",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.CreateTable(
                name: "ExpenseSharedUsers",
                columns: table => new
                {
                    ExpensesSharedId = table.Column<int>(type: "integer", nullable: false),
                    SharedWithUsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseSharedUsers", x => new { x.ExpensesSharedId, x.SharedWithUsersId });
                    table.ForeignKey(
                        name: "FK_ExpenseSharedUsers_Expenses_ExpensesSharedId",
                        column: x => x.ExpensesSharedId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseSharedUsers_Users_SharedWithUsersId",
                        column: x => x.SharedWithUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseSharedUsers_SharedWithUsersId",
                table: "ExpenseSharedUsers",
                column: "SharedWithUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_PaidByUserId",
                table: "Expenses",
                column: "PaidByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_PaidByUserId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ExpenseSharedUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_GroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PaidByUserId",
                table: "Expenses",
                newName: "PaidById");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_PaidByUserId",
                table: "Expenses",
                newName: "IX_Expenses_PaidById");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "ExpenseShares",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Expenses",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Expenses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "integer", nullable: false),
                    MembersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => new { x.GroupsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Users_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_MembersId",
                table: "GroupMembers",
                column: "MembersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_PaidById",
                table: "Expenses",
                column: "PaidById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
