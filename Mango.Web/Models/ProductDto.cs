using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty; 
        public string CategoryName { get; set; }
		public string? ImageUrl { get; set; }
		public string? ImageLocalPath { get; set; }
		public IFormFile? Image { get; set; }

		[Range(0, 100)]
        public int Qty { get; set; } = 1;
    }
}
