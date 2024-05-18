using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class AzureEmailDispatcher : IEmailDispatcher
    {
        private HttpClient _httpClient;
        private string _functionUrl;

        public AzureEmailDispatcher(HttpClient httpClient, string functionUrl)
        {
            _httpClient = httpClient;
            _functionUrl = functionUrl;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            var payload = new { email, subject, message };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_functionUrl, content);
            return response.IsSuccessStatusCode;
        }
    }
}
