using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mango.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "CategoryName", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Appetizers", "Ditch the cocktail sauce and serve this classic appetizer with a spicy twist", "https://food.fnr.sndimg.com/content/dam/images/food/fullset/2003/10/16/1/tm1b55_shrimp_cocktail_remoulade2.jpg.rend.hgtvcom.791.594.suffix/1400611025252.jpeg", "Shrim Cocktail", 12.9 },
                    { 2, "Appetizers", "Crab Cakes and 5-Ingredient Remoulade", "https://food.fnr.sndimg.com/content/dam/images/food/fullset/2022/01/11/0/WU3005_ree-drummond-crab-cakes-and-5-ingredient-remoulade_s4x3.jpg.rend.hgtvcom.791.594.suffix/1641930057492.jpeg", "Crab Cakes", 15.9 },
                    { 3, "Desserts", "Classic bread pudding is enhanced with coconut flakes and coconut milk.", "https://www.allrecipes.com/thmb/HF3Ylq-b_VE8_X4NfCdGL7nS3sk=/750x0/filters:no_upscale():max_bytes(150000):strip_icc():format(webp)/263143-0041adde94f1489496b5c578686a582f.jpg", "Coconut Bread Pudding", 9.9000000000000004 },
                    { 4, "Entree", "Crispy fish tacos with shredded cabbage and a spicy homemade white sauce", "https://www.allrecipes.com/thmb/mNCnzOIxW0A9CtgFRkrRC7qYWLk=/0x512/filters:no_upscale():max_bytes(150000):strip_icc():format(webp)/53729-fish-tacos-DDMFS-4x3-b5547c67c6f0432da06ad8f905e82c1e.jpg", "Fish Tacos", 16.899999999999999 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
