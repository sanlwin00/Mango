namespace Mango.Web;

public class StaticData
{
    public static string CouponApiBaseUrl { get; set; } = string.Empty;
    public static string AuthApiBaseUrl { get; set; } = string.Empty;
    public static string TokenCookieName { get; set; } = "JwtToken";

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
}
