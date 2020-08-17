using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private HttpClient _opsgenieClient;

        public SrvSlackNotifications(SlackSettings settings, IMessagesRepository messagesRepository, AppSettings globalSettings)
        {
            _settings = settings;
            _messagesRepository = messagesRepository;
            if (!string.IsNullOrEmpty(_settings.Env))
                _environment = $"Environment: {_settings.Env}";

            var opsgenieHost = globalSettings.SlackNotificationsJobSettings.OpsgenieHost;
            var opsgenieKey = globalSettings.SlackNotificationsJobSettings.OpsgenieApiKey;

            _opsgenieClient = new HttpClient();
            _opsgenieClient.DefaultRequestHeaders.Accept.Clear();
            _opsgenieClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _opsgenieClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("GenieKey", opsgenieKey);
            _opsgenieClient.BaseAddress = new Uri(opsgenieHost);
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

            var processedMessage = await ProcessMessageAsync(sender, message, channel);

            strBuilder.AppendLine(sender != null ? $"{sender} : {processedMessage}" : processedMessage);

            await HttpRequestClient.PostRequest(new { text = strBuilder.ToString() }.ToJson(), channel.WebHookUrl);

            if (channel.Opsgenie)
            {
                await PostRequest(sender, message);
            }
        }

        private async Task<string> ProcessMessageAsync(string sender, string message, SlackSettings.Channel channel)
        {
            var maxShortMessageLength = channel.MaxShortMessageLength == 0 ? 250 : channel.MaxShortMessageLength;

            if (message.Length <= maxShortMessageLength)
            {
                return message;
            }

            var fullMessageUrl = await _messagesRepository.SaveMessageAsync(_environment, sender, message);
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

        public async Task<string> PostRequest(string sender, string message)
        {
            var i = sender.IndexOf("]");
            var j = 0;

            while (i > 0)
            {
                j = i + 1;
                i = sender.IndexOf("]", j);
            }

            if (j > 0)
            {
                sender = sender.Substring(j);
            }

            sender = sender.Replace("\"", "\\\"");

            Console.WriteLine($"sender: {sender}");

            var json = $"{{ \"message\": \"{message}\", \"alias\": \"{sender}\", \"description\":\"Check in slack error\", \"priority\":\"P1\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _opsgenieClient.PostAsync("", content);

            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine(response.StatusCode != HttpStatusCode.Accepted
                ? $"Cannot send message to opsgenie. StatusCode: {response.StatusCode}; Body: {body}"
                : $"Sent to opsgenie, Alias: {sender}");

            return body;
        }
    }
}
