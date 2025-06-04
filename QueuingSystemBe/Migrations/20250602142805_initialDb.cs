using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueuingSystemBe.Migrations
{
    /// <inheritdoc />
    public partial class initialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Connected = table.Column<bool>(type: "boolean", nullable: true),
                    OperationStatus = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedUser = table.Column<string>(type: "text", nullable: true),
                    UpdatedUser = table.Column<string>(type: "text", nullable: true),
                    DeletedUser = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceCode);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsInOperation = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedUser = table.Column<string>(type: "text", nullable: true),
                    UpdatedUser = table.Column<string>(type: "text", nullable: true),
                    DeletedUser = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceCode);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Email = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedUser = table.Column<string>(type: "text", nullable: true),
                    UpdatedUser = table.Column<string>(type: "text", nullable: true),
                    DeletedUser = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignmentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpireDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: true),
                    ServiceCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DeviceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedUser = table.Column<string>(type: "text", nullable: true),
                    UpdatedUser = table.Column<string>(type: "text", nullable: true),
                    DeletedUser = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Assignments_Devices_DeviceCode",
                        column: x => x.DeviceCode,
                        principalTable: "Devices",
                        principalColumn: "DeviceCode",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Assignments_Services_ServiceCode",
                        column: x => x.ServiceCode,
                        principalTable: "Services",
                        principalColumn: "ServiceCode",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_DeviceCode",
                table: "Assignments",
                column: "DeviceCode");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ServiceCode",
                table: "Assignments",
                column: "ServiceCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
