using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextGame.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class tphtojsonstate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Items_ChestplateId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Items_CurrentMimicChestId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Items_HelmId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Items_WeaponId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Rooms_CurrentRoomId",
                table: "GameSessions");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Enemies");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_ChestplateId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_CurrentMimicChestId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_CurrentRoomId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_HelmId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_WeaponId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "ChestplateId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Coins",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CurrentHealth",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CurrentMimicChestId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CurrentRoomId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "HelmId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "IsGameStarted",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "IsInBattle",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Keys",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "MaxHealth",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "WeaponId",
                table: "GameSessions");

            migrationBuilder.AddColumn<byte[]>(
                name: "State",
                table: "GameSessions",
                type: "jsonb",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "GameSessions");

            migrationBuilder.AddColumn<int>(
                name: "ChestplateId",
                table: "GameSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Coins",
                table: "GameSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentHealth",
                table: "GameSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentMimicChestId",
                table: "GameSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRoomId",
                table: "GameSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HelmId",
                table: "GameSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGameStarted",
                table: "GameSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInBattle",
                table: "GameSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Keys",
                table: "GameSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxHealth",
                table: "GameSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeaponId",
                table: "GameSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDiscovered = table.Column<bool>(type: "boolean", nullable: false),
                    IsSearched = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoomType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enemies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    DamageBlock = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    EnemyType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enemies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enemies_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: true),
                    ChestId = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCarryable = table.Column<bool>(type: "boolean", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DamageBlock = table.Column<int>(type: "integer", nullable: true),
                    Durability = table.Column<int>(type: "integer", nullable: true),
                    Damage = table.Column<int>(type: "integer", nullable: true),
                    CurrentHealthBoost = table.Column<int>(type: "integer", nullable: true),
                    MaxHealthBoost = table.Column<int>(type: "integer", nullable: true),
                    MimicId = table.Column<int>(type: "integer", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Enemies_MimicId",
                        column: x => x.MimicId,
                        principalTable: "Enemies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Items_ChestId",
                        column: x => x.ChestId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_ChestplateId",
                table: "GameSessions",
                column: "ChestplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_CurrentMimicChestId",
                table: "GameSessions",
                column: "CurrentMimicChestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_CurrentRoomId",
                table: "GameSessions",
                column: "CurrentRoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_HelmId",
                table: "GameSessions",
                column: "HelmId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_WeaponId",
                table: "GameSessions",
                column: "WeaponId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_RoomId",
                table: "Enemies",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ChestId",
                table: "Items",
                column: "ChestId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_GameSessionId",
                table: "Items",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_MimicId",
                table: "Items",
                column: "MimicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_RoomId",
                table: "Items",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_GameSessionId",
                table: "Rooms",
                column: "GameSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Items_ChestplateId",
                table: "GameSessions",
                column: "ChestplateId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Items_CurrentMimicChestId",
                table: "GameSessions",
                column: "CurrentMimicChestId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Items_HelmId",
                table: "GameSessions",
                column: "HelmId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Items_WeaponId",
                table: "GameSessions",
                column: "WeaponId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Rooms_CurrentRoomId",
                table: "GameSessions",
                column: "CurrentRoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
