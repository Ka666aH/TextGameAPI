using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextGame.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class updatedeletebehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Items_WeaponId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_ChestId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Rooms_RoomId",
                table: "Items");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Items_WeaponId",
                table: "GameSessions",
                column: "WeaponId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_ChestId",
                table: "Items",
                column: "ChestId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Rooms_RoomId",
                table: "Items",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Items_WeaponId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_ChestId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Rooms_RoomId",
                table: "Items");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Items_WeaponId",
                table: "GameSessions",
                column: "WeaponId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_ChestId",
                table: "Items",
                column: "ChestId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Rooms_RoomId",
                table: "Items",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
