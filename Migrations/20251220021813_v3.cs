using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace micpix.Migrations
{
    /// <inheritdoc />
    public partial class v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collages_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Layers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollageId = table.Column<int>(type: "int", nullable: false),
                    LayerIndex = table.Column<int>(type: "int", nullable: false),
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    XOffset = table.Column<int>(type: "int", nullable: false),
                    YOffset = table.Column<int>(type: "int", nullable: false),
                    XScale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YScale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rotation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Opacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Layers_Collages_CollageId",
                        column: x => x.CollageId,
                        principalTable: "Collages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Layers_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultGIFs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollageId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrameCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultGIFs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultGIFs_Collages_CollageId",
                        column: x => x.CollageId,
                        principalTable: "Collages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collages_AuthorId",
                table: "Collages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Layers_CollageId",
                table: "Layers",
                column: "CollageId");

            migrationBuilder.CreateIndex(
                name: "IX_Layers_ResourceId",
                table: "Layers",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultGIFs_CollageId",
                table: "ResultGIFs",
                column: "CollageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Layers");

            migrationBuilder.DropTable(
                name: "ResultGIFs");

            migrationBuilder.DropTable(
                name: "Collages");
        }
    }
}
