using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Job.SlackNotifications.Core;
using Lykke.Job.SlackNotifications.Core.Domain;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class SlackNotifcationsConsumer
    {
        private readonly ISlackNotificationSender _srvSlackNotifications;
        private readonly INotificationFilter _notificationFilter;
        private readonly ILog _log;

        private DateTime _lastSendTime = DateTime.MinValue;
        private SlackNotificationRequestMsg _lastMsg;

        public SlackNotifcationsConsumer(
            ISlackNotificationSender srvSlackNotifications,
            INotificationFilter notificationFilter,
            ILog log)
        {
            _srvSlackNotifications = srvSlackNotifications;
            _notificationFilter = notificationFilter;
            _log = log;
        }

        [QueueTrigger("slack-notifications")]
        public async Task ProcessInMessage(SlackNotificationRequestMsg msg)
        {
            try
            {
                MuteItem muteItem = await _notificationFilter.GetMutedItem(msg);

                if (muteItem != null && string.IsNullOrEmpty(muteItem.Type))
                    return;

                var now = DateTime.UtcNow;
                if (_lastMsg != null && _lastMsg.Type == msg.Type
                    && _lastMsg.Sender == msg.Sender && _lastMsg.Message == msg.Message
                    && now.Subtract(_lastSendTime).TotalMinutes < 1)
                    return;

                await _srvSlackNotifications.SendNotificationAsync(muteItem?.Type ?? msg.Type, msg.Message, msg.Sender);

                _lastMsg = msg;
                _lastSendTime = now;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SlackNotificationRequestsConsumer", "ProcessInMessage", msg.ToJson(), ex);
                throw;
            }
        }
    }
}
