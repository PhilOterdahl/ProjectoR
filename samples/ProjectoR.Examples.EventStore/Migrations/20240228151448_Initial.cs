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
                name: "AmountOfStudentsPerCity",
                schema: "Projection",
                columns: table => new
                {
                    City = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmountOfStudentsPerCity", x => x.City);
                });

            migrationBuilder.CreateTable(
                name: "AmountOfStudentsPerCountry",
                schema: "Projection",
                columns: table => new
                {
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmountOfStudentsPerCountry", x => x.CountryCode);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                schema: "Projection",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Student", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmountOfStudentsPerCity",
                schema: "Projection");

            migrationBuilder.DropTable(
                name: "AmountOfStudentsPerCountry",
                schema: "Projection");

            migrationBuilder.DropTable(
                name: "Student",
                schema: "Projection");
        }
    }
}
