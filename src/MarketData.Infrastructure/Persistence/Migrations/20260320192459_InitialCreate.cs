using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AssetKind = table.Column<string>(type: "text", nullable: false),
                    Exchange = table.Column<string>(type: "text", nullable: false),
                    LastPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ExternalId",
                table: "Assets",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
