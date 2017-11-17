using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Common;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Domain;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class SrvSlackNotifications : ISlackNotificationSender
    {
        private const string _unknownSender = "unknown sender";
        private const string _unknownEnv = "Unknown env";
        private const string _warning = "Warning";

        private readonly SlackSettings _settings;
        private readonly IMessagesRepository _messagesRepository;
        private readonly string _environment;

        public SrvSlackNotifications(SlackSettings settings, IMessagesRepository messagesRepository)
        {
            _settings = settings;
            _messagesRepository = messagesRepository;
            if (!string.IsNullOrEmpty(_settings.Env))
                _environment = $"Environment: {_settings.Env}";
        }

        public async Task SendNotificationAsync(string type, string message, string sender = null)
        {
            var channel = await GetChannelAsync(type, sender);
            if (channel == null)
            {
                return;
            }

            var strBuilder = new StringBuilder();
            if (_environment != null)
                strBuilder.AppendLine(_environment);

            var processedMessage = await ProcessMessageAsync(message, channel);

            strBuilder.AppendLine(sender != null ? $"{sender} : {processedMessage}" : processedMessage);

            await HttpRequestClient.PostRequest(new { text = strBuilder.ToString() }.ToJson(), channel.WebHookUrl);
        }

        private async Task<string> ProcessMessageAsync(string message, SlackSettings.Channel channel)
        {
            var maxShortMessageLength = channel.MaxShortMessageLength == 0 ? 250 : channel.MaxShortMessageLength;

            if (message.Length <= maxShortMessageLength)
            {
                return message;
            }

            var fullMessageUrl = await _messagesRepository.SaveMessageAsync(message);
            return $"{message.Substring(0, maxShortMessageLength)}... <{fullMessageUrl}|Read all>";
        }

        private async Task<SlackSettings.Channel> GetChannelAsync(string type, string sender)
        {
            var channel = _settings.Channels.FirstOrDefault(x => x.Type == type);
            if (!string.IsNullOrWhiteSpace(channel?.WebHookUrl))
            {
                return channel;
            }

            channel = _settings.Channels.FirstOrDefault(x => x.Type == _warning);

            if (string.IsNullOrWhiteSpace(channel?.WebHookUrl))
            {
                channel = _settings.Channels.FirstOrDefault(x => x.Type.StartsWith(_warning));
            }

            if (!string.IsNullOrWhiteSpace(channel?.WebHookUrl))
            {
                if (string.IsNullOrWhiteSpace(sender))
                {
                    sender = _unknownSender;
                }

                var env = !string.IsNullOrEmpty(_settings.Env)
                    ? _settings.Env
                    : _unknownEnv;

                await HttpRequestClient.PostRequest(new {text = $"{env}: Couldn't find webhook for {type} from {sender}"}.ToJson(), channel.WebHookUrl);
            }

            return null;
        }
    }
}
