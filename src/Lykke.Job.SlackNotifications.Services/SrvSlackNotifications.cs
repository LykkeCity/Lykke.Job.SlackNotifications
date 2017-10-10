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
        private const string _unknownSender = "unknown sender";
        private const string _unknownEnv = "Unknown env";
        private const string _warning = "Warning";

        private readonly SlackSettings _settings;
        private readonly string _environment;

        public SrvSlackNotifications(SlackSettings settings)
        {
            _settings = settings;
            if (!string.IsNullOrEmpty(_settings.Env))
                _environment = $"Environment: {_settings.Env}";
        }

        public async Task SendNotificationAsync(string type, string message, string sender = null)
        {
            var webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
            if (webHookUrl == null)
            {
                webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type == _warning)?.WebHookUrl;
                if (webHookUrl == null)
                    webHookUrl = _settings.Channels.FirstOrDefault(x => x.Type.StartsWith(_warning))?.WebHookUrl;
                if (webHookUrl != null)
                {
                    if (string.IsNullOrWhiteSpace(sender))
                        sender = _unknownSender;
                    string env = _unknownEnv;
                    if (!string.IsNullOrEmpty(_settings.Env))
                        env = _settings.Env;
                    await HttpRequestClient.PostRequest(
                        new { text = $"{env}: Couldn't find webhook for {type} from {sender}" }.ToJson(), webHookUrl);
                }
                return;
            }

            var strBuilder = new StringBuilder();
            if (_environment != null)
                strBuilder.AppendLine(_environment);
            strBuilder.AppendLine(sender != null ? $"{sender} : {message}" : message);

            await HttpRequestClient.PostRequest(new { text = strBuilder.ToString() }.ToJson(), webHookUrl);
        }
    }
}
