using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjectoR.Examples.CustomSubscription.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Projection");

            migrationBuilder.EnsureSchema(
                name: "ProjectoR");

            migrationBuilder.CreateTable(
                name: "AmountOfUsersPerCity",
                schema: "Projection",
                columns: table => new
                {
                    City = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmountOfUsersPerCity", x => x.City);
                });

            migrationBuilder.CreateTable(
                name: "AmountOfUsersPerCountry",
                schema: "Projection",
                columns: table => new
                {
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmountOfUsersPerCountry", x => x.CountryCode);
                });

            migrationBuilder.CreateTable(
                name: "Checkpoint",
                schema: "ProjectoR",
                columns: table => new
                {
                    ProjectionName = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkpoint", x => x.ProjectionName);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StreamName = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Projection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    ContactInformation_Email = table.Column<string>(type: "text", nullable: false),
                    ContactInformation_Mobile = table.Column<string>(type: "text", nullable: false),
                    Address_CountryCode = table.Column<string>(type: "text", nullable: false),
                    Address_PostalCode = table.Column<string>(type: "text", nullable: false),
                    Address_City = table.Column<string>(type: "text", nullable: false),
                    Address_Street = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_EventName",
                table: "Event",
                column: "EventName");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Position",
                table: "Event",
                column: "Position",
                unique: true,
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Event_StreamName",
                table: "Event",
                column: "StreamName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmountOfUsersPerCity",
                schema: "Projection");

            migrationBuilder.DropTable(
                name: "AmountOfUsersPerCountry",
                schema: "Projection");

            migrationBuilder.DropTable(
                name: "Checkpoint",
                schema: "ProjectoR");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Projection");
        }
    }
}
