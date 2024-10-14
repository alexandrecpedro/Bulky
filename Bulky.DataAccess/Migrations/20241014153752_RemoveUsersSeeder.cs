using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bulky.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUsersSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0ed56266-b859-474f-95dc-a60e217f7554");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1cf673ca-2c15-461d-bded-7316e4fc96c4");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "611e5aa3-fcbe-4a78-b707-41239a3173b5");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "db8d8aa5-bf8f-4e7c-83d3-39d5b16b5cb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "City", "CompanyId", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PostalCode", "SecurityStamp", "State", "StreetAddress", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "0ed56266-b859-474f-95dc-a60e217f7554", 0, "Chicago", 2, "06d6e51c-e48f-4f5e-8ce9-26965690f79e", "ApplicationUser", "testcompany@gmail.com", false, false, null, "Ben Peter (Company)", null, null, null, "7735457827", false, "60634", "ce8c5864-02ea-4a07-b3f6-4bbe530cbbae", "IL", "3435 N Central Ave", false, "testcompany@gmail.com" },
                    { "1cf673ca-2c15-461d-bded-7316e4fc96c4", 0, "Chicago", null, "8810a68c-2970-4379-aae6-d033b34ffd7b", "ApplicationUser", "applicatonuser@gmail.com", false, false, null, "Ben Peter (ApplicationUser)", null, null, null, "7735457827", false, "60634", "0c06e2c1-6dfd-4038-876e-8ad26920af2e", "IL", "3435 N Central Ave", false, "applicatonuser@gmail.com" },
                    { "611e5aa3-fcbe-4a78-b707-41239a3173b5", 0, "Chicago", null, "9dc740d7-ea20-43f9-bc1b-50b60b448a53", "ApplicationUser", "employee2@gmail.com", false, false, null, "Jess Pat", null, null, null, "7735457827", false, "60634", "3a99ecc4-14e1-4015-aa8f-0c261625c8c5", "IL", "3435 N Central Ave", false, "employee2@gmail.com" },
                    { "db8d8aa5-bf8f-4e7c-83d3-39d5b16b5cb4", 0, "Chicago", null, "db50409a-dfb4-4192-877e-c90492622a27", "ApplicationUser", "testemployee@gmail.com", false, false, null, "Ben Peter (Employee)", null, null, null, "7735457827", false, "60634", "e8522789-3f4a-407a-8c4e-58264f745789", "IL", "3435 N Central Ave", false, "testemployee@gmail.com" }
                });
        }
    }
}
