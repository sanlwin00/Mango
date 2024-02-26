

namespace Mango.Services.OrderAPI.Models.Dto
{
    public record StripeRequestDto(string SessionId, string SessionUrl, string ApprovedUrl, string CancelUrl, OrderHeaderDto OrderHeader);
}
