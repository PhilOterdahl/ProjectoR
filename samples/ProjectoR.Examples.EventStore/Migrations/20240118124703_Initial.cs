using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectoR.Examples.EventStore.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Projection");

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
                name: "User",
                schema: "Projection");
        }
    }
}
