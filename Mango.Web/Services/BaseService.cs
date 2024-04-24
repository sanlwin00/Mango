
using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Mango.Web.Services;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;
    public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
    }
    public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
    {
        HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
        HttpRequestMessage message = new();
        
        if (withBearer) 
        {
            var token = _tokenProvider.GetToken();
            message.Headers.Add("Authorization", $"Bearer {token}");
        }

        message.RequestUri = new Uri(requestDto.Url);

		if (requestDto.ContentType == StaticData.ContentType.MultipartFormData)
		{
			message.Headers.Add("Accept", "*/*");
            var content = new MultipartFormDataContent();
            foreach(var prop in requestDto.Data.GetType().GetProperties())
            {
                var value = prop.GetValue(requestDto.Data);
                if (value is FormFile)
                {
                    var file = (FormFile)value;
                    if (file!=null)
                    {
                        content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                    }
                }
                else if(value != null)
                {                    
                    content.Add(new StringContent(value.ToString()), prop.Name);
                }
            }
			message.Content = content;
		}
		else
		{
			message.Headers.Add("Accept", "application/json");
			if (requestDto.Data != null)
			{
				message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
			}
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
            var responseBody = await response.Content.ReadAsStringAsync();
            switch (response.StatusCode)
            {                
                case HttpStatusCode.NotFound:
                    return new() { IsSuccess = false, Message = "Not Found" };
                case HttpStatusCode.Forbidden:
                    return new() { IsSuccess = false, Message = "Access Denied" };
                case HttpStatusCode.Unauthorized:
                    return new() { IsSuccess = false, Message = "Unauthorized" };
                case HttpStatusCode.InternalServerError:
                    return new() { IsSuccess = false, Message = "Internal Server Error!\n" + responseBody?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() };
                case HttpStatusCode.OK:
                    var apiContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                case HttpStatusCode.BadRequest:
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(content);
                    return new() { IsSuccess = false, Message = response.ReasonPhrase + " | " + obj.title +" | " + obj.message };
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
