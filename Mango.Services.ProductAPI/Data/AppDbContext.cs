using Mango.Services.ProductAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 1,
                Name = "Shrimp Cocktail",
                Price = 12.90,
                CategoryName = "Appetizers",
                Description = "Ditch the cocktail sauce and serve this classic appetizer with a spicy twist",
                ImageUrl = "https://food.fnr.sndimg.com/content/dam/images/food/fullset/2003/10/16/1/tm1b55_shrimp_cocktail_remoulade2.jpg.rend.hgtvcom.791.594.suffix/1400611025252.jpeg"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 2,
                Name = "Crab Cakes",
                Price = 15.90,
                CategoryName = "Appetizers",
                Description = "Crab Cakes and 5-Ingredient Remoulade",
                ImageUrl = "https://food.fnr.sndimg.com/content/dam/images/food/fullset/2022/01/11/0/WU3005_ree-drummond-crab-cakes-and-5-ingredient-remoulade_s4x3.jpg.rend.hgtvcom.791.594.suffix/1641930057492.jpeg"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 3,
                Name = "Coconut Bread Pudding",
                Price = 9.90,
                CategoryName = "Desserts",
                Description = "Classic bread pudding is enhanced with coconut flakes and coconut milk.",
                ImageUrl = "https://www.allrecipes.com/thmb/HF3Ylq-b_VE8_X4NfCdGL7nS3sk=/750x0/filters:no_upscale():max_bytes(150000):strip_icc():format(webp)/263143-0041adde94f1489496b5c578686a582f.jpg"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 4,
                Name = "Fish Tacos",
                Price = 16.90,
                CategoryName = "Entree",
                Description = "Crispy fish tacos with shredded cabbage and a spicy homemade white sauce",
                ImageUrl = "https://www.allrecipes.com/thmb/mNCnzOIxW0A9CtgFRkrRC7qYWLk=/0x512/filters:no_upscale():max_bytes(150000):strip_icc():format(webp)/53729-fish-tacos-DDMFS-4x3-b5547c67c6f0432da06ad8f905e82c1e.jpg"
            });
        }
    }
}

