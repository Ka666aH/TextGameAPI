using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextGame.Infrastructure.Database.Migrations
{
    // Ручная миграция: Author не может запускать dotnet ef (notes/AGENTS.md §1).
    // Diff-источник для будущих миграций — AppDbContextModelSnapshot, он обновлён вместе с этим файлом.
    [DbContext(typeof(TextGame.Infrastructure.Database.AppDbContext))]
    [Migration("20261116140000_add-seed-to-session")]
    public partial class AddSeedToSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Seed",
                table: "Sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seed",
                table: "Sessions");
        }
    }
}
