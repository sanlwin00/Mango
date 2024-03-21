using Mango.Web.Models.Dto;

namespace Mango.Web.Models
{
    public class StripeRequestDto
    {
        public string? SessionId { get; set; }
        public string? SessionUrl { get; set; }
        public string ApprovedUrl { get; set; }
        public string CancelUrl { get; set; }
        public OrderHeaderDto OrderHeader { get; set; }
    }

}
