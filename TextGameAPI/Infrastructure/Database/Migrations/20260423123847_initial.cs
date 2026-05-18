using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextGame.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HashedPass = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enemies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    DamageBlock = table.Column<int>(type: "integer", nullable: false),
                    EnemyType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enemies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentRoomId = table.Column<int>(type: "integer", nullable: true),
                    WeaponId = table.Column<int>(type: "integer", nullable: false),
                    HelmId = table.Column<int>(type: "integer", nullable: true),
                    ChestplateId = table.Column<int>(type: "integer", nullable: true),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false),
                    CurrentHealth = table.Column<int>(type: "integer", nullable: false),
                    Coins = table.Column<int>(type: "integer", nullable: false),
                    Keys = table.Column<int>(type: "integer", nullable: false),
                    IsGameStarted = table.Column<bool>(type: "boolean", nullable: false),
                    IsInBattle = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentMimicChestId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    IsDiscovered = table.Column<bool>(type: "boolean", nullable: false),
                    IsSearched = table.Column<bool>(type: "boolean", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
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
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<int>(type: "integer", nullable: true),
                    IsCarryable = table.Column<bool>(type: "boolean", nullable: false),
                    ChestId = table.Column<int>(type: "integer", nullable: true),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Durability = table.Column<int>(type: "integer", nullable: true),
                    DamageBlock = table.Column<int>(type: "integer", nullable: true),
                    Damage = table.Column<int>(type: "integer", nullable: true),
                    MaxHealthBoost = table.Column<int>(type: "integer", nullable: true),
                    CurrentHealthBoost = table.Column<int>(type: "integer", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: true),
                    MimicId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
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
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_RoomId",
                table: "Enemies",
                column: "RoomId");

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
                name: "IX_GameSessions_UserId",
                table: "GameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_WeaponId",
                table: "GameSessions",
                column: "WeaponId",
                unique: true);

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
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_GameSessionId",
                table: "Rooms",
                column: "GameSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enemies_Rooms_RoomId",
                table: "Enemies",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Rooms_CurrentRoomId",
                table: "GameSessions",
                column: "CurrentRoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enemies_Rooms_RoomId",
                table: "Enemies");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Rooms_CurrentRoomId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Rooms_RoomId",
                table: "Items");

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

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Enemies");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
