namespace Mango.Web;

public class StaticData
{
    public static string CouponApiBaseUrl { get; set; } = string.Empty;
    public static string AuthApiBaseUrl { get; set; } = string.Empty;
    public static string TokenCookieName { get; set; } = "JwtToken";
    public static string ProductApiBaseUrl { get; set; } = string.Empty;
    public static string ShoppingCartApiBaseUrl { get; set; } = string.Empty;
    public static string OrderApiBaseUrl { get; set; } = string.Empty;

    public enum Roles
    {
        ADMIN,
        CUSTOMER
    }
    public enum ApiType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
    public enum OrderStatus
    {
        Pending,
        Approved,
        ReadyForPickup,
        Completed,
        Refunded,
        Cancelled
    }
}
