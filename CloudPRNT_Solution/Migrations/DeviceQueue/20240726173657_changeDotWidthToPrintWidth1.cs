using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudPRNT_Solution.Migrations.DeviceQueue
{
    /// <inheritdoc />
    public partial class changeDotWidthToPrintWidth1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DotWidth",
                table: "DeviceTable");

            migrationBuilder.AddColumn<string>(
                name: "PrintWidth",
                table: "DeviceTable",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintWidth",
                table: "DeviceTable");

            migrationBuilder.AddColumn<int>(
                name: "DotWidth",
                table: "DeviceTable",
                type: "INTEGER",
                nullable: true);
        }
    }
}
