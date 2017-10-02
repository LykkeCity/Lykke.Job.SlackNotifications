using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Common;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class SrvSlackNotifications : ISlackNotificationSender
    {
        private readonly SlackSettings _settings;

        public SrvSlackNotifications(SlackSettings settings)
        {
            _settings = settings;
        }

        public async Task SendNotificationAsync(string type, string message, string sender = null)
        {
            var webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
            if (webHookUrl == null)
            {
                webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type == "Warning")?.WebHookUrl;
                if (webHookUrl == null)
                    webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type.StartsWith("Warning"))?.WebHookUrl;
                if (webHookUrl != null)
                {
                    if (string.IsNullOrWhiteSpace(sender))
                        sender = "unknown sender";
                    await HttpRequestClient.PostRequest(
                        new { text = $"Couldn't find webhook for {type} from {sender}" }.ToJson(), webHookUrl);
                }
                return;
            }

            var strBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(_settings.Env))
                strBuilder.AppendLine($"Environment: {_settings.Env}");
            strBuilder.AppendLine(sender != null ? $"{sender} : {message}" : message);

            await HttpRequestClient.PostRequest(new { text = strBuilder.ToString() }.ToJson(), webHookUrl);
        }
    }
}
