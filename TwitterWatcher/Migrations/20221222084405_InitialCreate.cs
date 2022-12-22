using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitterWatcher.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    twitterid = table.Column<string>(name: "twitter_id", type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    lastprocessedtweetid = table.Column<string>(name: "last_processed_tweet_id", type: "text", nullable: true),
                    lastprocessedtime = table.Column<DateTimeOffset>(name: "last_processed_time", type: "timestamp with time zone", nullable: false),
                    lasttweettime = table.Column<DateTimeOffset>(name: "last_tweet_time", type: "timestamp with time zone", nullable: false),
                    lastmediatweettime = table.Column<DateTimeOffset>(name: "last_media_tweet_time", type: "timestamp with time zone", nullable: false),
                    usernamehistory = table.Column<string[]>(name: "username_history", type: "text[]", nullable: true),
                    isactive = table.Column<bool>(name: "is_active", type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artists", x => x.twitterid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artists");
        }
    }
}
