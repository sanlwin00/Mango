namespace Mango.Services.OrderAPI.Models.Dto
{
    public class CartDetailDto
    {
        public int CartDetailId { get; set; }
        public int CartHeaderId { get; set; }
        public CartHeaderDto? CartHeader { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }
        public int Qty { get; set; }
    }
}
