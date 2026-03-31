using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <summary>
/// Canonical (min,max) user pair + merge duplicate threads + unique index — one DM per pair.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260330143000_ConversationOnePerPair")]
public class ConversationOnePerPair : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1) Store participants in consistent order (smaller Id first).
        migrationBuilder.Sql(
            """
            UPDATE "Conversations"
            SET "User1Id" = LEAST("User1Id", "User2Id"),
                "User2Id" = GREATEST("User1Id", "User2Id");
            """);

        // 2) Point messages at the keeper row (lowest Conversation Id for each pair).
        migrationBuilder.Sql(
            """
            UPDATE "Messages" AS m
            SET "ConversationId" = sub."keeper_id"
            FROM (
              SELECT c."Id" AS conv_id,
                     MIN(c."Id") OVER (PARTITION BY c."User1Id", c."User2Id") AS keeper_id
              FROM "Conversations" c
            ) AS sub
            WHERE m."ConversationId" = sub.conv_id
              AND sub.conv_id <> sub.keeper_id;
            """);

        // 3) Remove duplicate conversation rows (same pair).
        migrationBuilder.Sql(
            """
            DELETE FROM "Conversations" AS c
            USING "Conversations" AS c2
            WHERE c."User1Id" = c2."User1Id"
              AND c."User2Id" = c2."User2Id"
              AND c."Id" > c2."Id";
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Conversations_User1Id_User2Id",
            table: "Conversations",
            columns: new[] { "User1Id", "User2Id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Conversations_User1Id_User2Id",
            table: "Conversations");
    }
}
