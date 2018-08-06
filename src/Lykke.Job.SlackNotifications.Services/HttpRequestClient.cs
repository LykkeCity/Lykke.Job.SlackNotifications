using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.SlackNotifications.Services
{
    internal sealed class HttpRequestClient
    {
        private static readonly HttpClient _instance = new HttpClient();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static HttpRequestClient()
        {
            _instance.DefaultRequestHeaders.Accept.Clear();
            _instance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        HttpRequestClient(){}

        public static async Task<string> PostRequest(string json, string url)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _instance.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
