
using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Mango.Web.Services;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public BaseService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
    {
        HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
        HttpRequestMessage message = new();
        message.Headers.Add("Accept", "application/json");
        // token
        message.RequestUri = new Uri(requestDto.Url);
        if (requestDto.Data != null)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
        }

        switch (requestDto.ApiType)
        {
            case StaticData.ApiType.POST:
                message.Method = HttpMethod.Post;
                break;
            case StaticData.ApiType.PUT:
                message.Method = HttpMethod.Put;
                break;
            case StaticData.ApiType.DELETE:
                message.Method = HttpMethod.Delete;
                break;
            default:
                message.Method = HttpMethod.Get;
                break;
        }
        try
        {
            HttpResponseMessage response = await client.SendAsync(message);
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new() { IsSuccess = false, Message = "Not Found" };
                case HttpStatusCode.Forbidden:
                    return new() { IsSuccess = false, Message = "Access Denied" };
                case HttpStatusCode.Unauthorized:
                    return new() { IsSuccess = false, Message = "Unauthorized" };
                case HttpStatusCode.InternalServerError:
                    return new() { IsSuccess = false, Message = "InternalServerError" };
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.OK:
                    var apiContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                default:
                    return new() { IsSuccess = false, Message = response.ReasonPhrase };

            }
        }
        catch (Exception ex)
        {
            return new() { IsSuccess = false, Message = ex.Message };
        }
    }
}
