using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudPRNT_Solution.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenDrawerColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenDrawer",
                table: "PrintQueue",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenDrawer",
                table: "PrintQueue");
        }
    }
}
