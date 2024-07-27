using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudPRNT_Solution.Migrations.DeviceQueue
{
    /// <inheritdoc />
    public partial class changeDotWidthToPrintWidth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DotWidth",
                table: "DeviceTable",
                type: "TEXT",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DotWidth",
                table: "DeviceTable",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
