using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slot.Adapters.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Addedmorefieldsincoversationtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_GroundOwnerId1",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_UserId1",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_GroundOwnerId1",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "GroundOwnerId1",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Conversations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroundOwnerId1",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_GroundOwnerId1",
                table: "Conversations",
                column: "GroundOwnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_GroundOwnerId1",
                table: "Conversations",
                column: "GroundOwnerId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_UserId1",
                table: "Conversations",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
