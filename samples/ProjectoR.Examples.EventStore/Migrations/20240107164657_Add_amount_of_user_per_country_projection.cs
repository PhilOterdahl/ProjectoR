using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectoR.Examples.EventStore.Migrations
{
    /// <inheritdoc />
    public partial class Add_amount_of_user_per_country_projection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_CountryCode",
                schema: "Projection",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmountOfUsersPerCountry",
                schema: "Projection");

            migrationBuilder.DropColumn(
                name: "Address_CountryCode",
                schema: "Projection",
                table: "User");
        }
    }
}
