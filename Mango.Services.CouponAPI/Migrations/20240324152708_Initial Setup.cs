using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mango.Services.CouponAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CouponId",
                keyValue: "99a7dfec-1aa6-4cc8-8791-29c283021163");

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CouponId",
                keyValue: "b798c040-fcd1-4563-a97c-a028ee8921af");

            migrationBuilder.InsertData(
                table: "Coupons",
                columns: new[] { "CouponId", "CouponCode", "DiscountAmount", "MinAmount" },
                values: new object[,]
                {
                    { "465a1af3-c488-4ada-bd18-2cc09566ec88", "10OFF", 10.0, 100 },
                    { "704ef7ef-646d-4bcb-8605-d9e51a62b309", "20OFF", 20.0, 150 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CouponId",
                keyValue: "465a1af3-c488-4ada-bd18-2cc09566ec88");

            migrationBuilder.DeleteData(
                table: "Coupons",
                keyColumn: "CouponId",
                keyValue: "704ef7ef-646d-4bcb-8605-d9e51a62b309");

            migrationBuilder.InsertData(
                table: "Coupons",
                columns: new[] { "CouponId", "CouponCode", "DiscountAmount", "MinAmount" },
                values: new object[,]
                {
                    { "99a7dfec-1aa6-4cc8-8791-29c283021163", "10OFF", 10.0, 100 },
                    { "b798c040-fcd1-4563-a97c-a028ee8921af", "20OFF", 20.0, 150 }
                });
        }
    }
}
