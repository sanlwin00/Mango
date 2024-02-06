namespace Mango.Web;

public class StaticData
{
    public static string CouponApiBaseUrl { get; set; } = string.Empty;
    public enum ApiType
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}
