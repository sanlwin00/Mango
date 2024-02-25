namespace Mango.Web.Models.Dto
{
    public class CartHeaderDto
    {        
        public int CartHeaderId { get; set; }
        public string UserId { get; set; }
        public double Discount { get; set; }
        public string? CouponCode { get; set; } = string.Empty;
        public double CartTotal { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}


