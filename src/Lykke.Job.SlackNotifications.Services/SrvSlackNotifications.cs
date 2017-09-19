using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Common;
using Lykke.Job.SlackNotifications.Core;

namespace Lykke.Job.SlackNotifications.Services
{
    public class SrvSlackNotifications
    {
        private readonly SlackSettings _settings;

        public SrvSlackNotifications(SlackSettings settings)
        {
            _settings = settings;
        }

        public async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
            if (webHookUrl != null)
            {
                var text = new StringBuilder();

                if (!string.IsNullOrEmpty(_settings.Env))
                    text.AppendLine($"Environment: {_settings.Env}");

                text.AppendLine(sender != null ? $"{sender} : {message}" : message);

                await HttpRequestClient.PostRequest(new { text = text.ToString() }.ToJson(), webHookUrl);
            }
        }
    }
}
