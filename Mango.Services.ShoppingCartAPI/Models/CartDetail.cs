using Mango.Services.ShoppingCartAPI.Models.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartDetail
    {
        public int CartDetailId { get; set; }
        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public CartHeader CartHeader { get; set; }
        public int ProductId { get; set; }
        [NotMapped]
        public ProductDto Product{ get; set; }
        public int Qty { get; set; }
    }
}
