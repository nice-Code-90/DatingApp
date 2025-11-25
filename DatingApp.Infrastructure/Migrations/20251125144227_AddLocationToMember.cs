using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Location_M",
                table: "Members",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Location_SRID",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_X",
                table: "Members",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Y",
                table: "Members",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Z",
                table: "Members",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-id",
                column: "ConcurrencyStamp",
                value: "0b17be31-4711-403c-aa83-ab5ebc0c208f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "member-id",
                column: "ConcurrencyStamp",
                value: "8977a9ea-210b-4ec3-a3cb-8e331f73135f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "moderator-id",
                column: "ConcurrencyStamp",
                value: "4d79d2c0-af38-4bf7-880f-7288e387668a");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_M",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Location_SRID",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Location_X",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Location_Y",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Location_Z",
                table: "Members");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-id",
                column: "ConcurrencyStamp",
                value: "07d7a921-6bfb-4987-b4d9-4284ef7e0c80");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "member-id",
                column: "ConcurrencyStamp",
                value: "66714cde-010a-445d-a641-568a4c15fb71");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "moderator-id",
                column: "ConcurrencyStamp",
                value: "fc5acbf2-e356-4751-8577-e73a72fc7cd1");
        }
    }
}
