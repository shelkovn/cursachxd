using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace micpix.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTableAndAuthorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Users table
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // Add AuthorId column to Resources (nullable first)
            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Resources",
                type: "int",
                nullable: true);

            // Create default user "Rem"
            migrationBuilder.Sql(@"
            INSERT INTO Users (Username, RegistrationDate) 
            VALUES ('Rem', GETDATE())
        ");

            // Update all existing resources to use the default user (ID = 1)
            migrationBuilder.Sql(@"
            UPDATE Resources SET AuthorId = 1
        ");

            // Make AuthorId non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Create foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Resources_AuthorId",
                table: "Resources",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Users_AuthorId",
                table: "Resources",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourcesSet");

            migrationBuilder.DropTable(
                name: "UserSet");
        }
    }
}
