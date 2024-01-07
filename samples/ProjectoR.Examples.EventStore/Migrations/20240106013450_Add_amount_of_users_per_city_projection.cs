using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectoR.Examples.EventStore.Migrations
{
    /// <inheritdoc />
    public partial class Add_amount_of_users_per_city_projection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmountOfUsersPerCity",
                schema: "Projection");
        }
    }
}
