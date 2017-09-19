﻿using System.Linq;
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
                return;

            var strBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(_settings.Env))
                strBuilder.AppendLine($"Environment: {_settings.Env}");
            strBuilder.AppendLine(sender != null ? $"{sender} : {message}" : message);

            await HttpRequestClient.PostRequest(new { text = strBuilder.ToString() }.ToJson(), webHookUrl);
        }
    }
}
