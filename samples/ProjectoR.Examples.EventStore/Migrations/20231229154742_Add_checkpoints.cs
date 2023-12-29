using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectoR.Examples.EventStore.Migrations
{
    /// <inheritdoc />
    public partial class Add_checkpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ProjectoR");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checkpoint",
                schema: "ProjectoR");
        }
    }
}
