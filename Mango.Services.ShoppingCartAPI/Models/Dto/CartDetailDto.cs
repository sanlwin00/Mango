namespace Mango.Services.ShoppingCartAPI.Models.Dto
{
    public class CartDetailDto
    {
        public int CartDetailId { get; set; }
        public int CartId { get; set; }
        public CartHeader CartHeader { get; set; }
        public int ProductId { get; set; }
        public ProductDto Product { get; set; }
        public int Qty { get; set; }
    }
}
