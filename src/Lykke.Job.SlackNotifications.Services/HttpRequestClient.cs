using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.SlackNotifications.Services
{
    internal sealed class HttpRequestClient
    {
        private static readonly HttpClient instance = new HttpClient();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static HttpRequestClient(){}

        HttpRequestClient(){}

        public static HttpClient Instance => instance;
        
        public static async Task<string> PostRequest(string json, string url)
        {
            Instance.DefaultRequestHeaders.Accept.Clear();
            Instance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Instance.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
