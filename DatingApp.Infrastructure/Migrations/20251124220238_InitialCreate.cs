using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-id",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "member-id",
                column: "ConcurrencyStamp",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "moderator-id",
                column: "ConcurrencyStamp",
                value: null);
        }
    }
}
