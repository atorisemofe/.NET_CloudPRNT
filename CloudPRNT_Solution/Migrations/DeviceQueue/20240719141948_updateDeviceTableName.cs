﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudPRNT_Solution.Migrations.DeviceQueue
{
    /// <inheritdoc />
    public partial class updateDeviceTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceQueue");

            migrationBuilder.CreateTable(
                name: "DeviceTable",
                columns: table => new
                {
                    PrinterMac = table.Column<string>(type: "TEXT", nullable: true),
                    QueueID = table.Column<int>(type: "INTEGER", nullable: false),
                    DotWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    ClientType = table.Column<string>(type: "TEXT", nullable: true),
                    ClientVersion = table.Column<string>(type: "TEXT", nullable: true),
                    LastPoll = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceTable");

            migrationBuilder.CreateTable(
                name: "DeviceQueue",
                columns: table => new
                {
                    ClientType = table.Column<string>(type: "TEXT", nullable: true),
                    ClientVersion = table.Column<string>(type: "TEXT", nullable: true),
                    DotWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPoll = table.Column<string>(type: "TEXT", nullable: true),
                    PrinterMac = table.Column<string>(type: "TEXT", nullable: true),
                    QueueID = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                });
        }
    }
}
