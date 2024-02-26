using Mango.Web.Models.Dto;

namespace Mango.Web.Models
{
    public record StripeRequestDto(string SessionId, string SessionUrl, string ApprovedUrl, string CancelUrl, OrderHeaderDto OrderHeader);
}
