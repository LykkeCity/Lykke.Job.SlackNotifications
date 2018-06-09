using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
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

        public SlackNotifcationsConsumer(
            ISlackNotificationSender srvSlackNotifications,
            INotificationFilter notificationFilter,
            ILog log)
        {
            _srvSlackNotifications = srvSlackNotifications;
            _notificationFilter = notificationFilter;
            _log = log;
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications", timeoutInSeconds: 100)]
        public Task ProcessGeneralMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-critical", timeoutInSeconds: 100)]
        public Task ProcessCriticalMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-critical");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-debug", timeoutInSeconds: 100)]
        public Task ProcessDebugMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-debug");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-error", timeoutInSeconds: 100)]
        public Task ProcessErrorMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-error");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-information", timeoutInSeconds: 100)]
        public Task ProcessInfoMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-information");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-monitor", timeoutInSeconds: 100)]
        public Task ProcessMonitorMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-monitor");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-poison", timeoutInSeconds: 100)]
        public Task ProcessPoisonMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-poison");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-trace", timeoutInSeconds: 100)]
        public Task ProcessTraceMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-trace");
        }

        [UsedImplicitly]
        [QueueTrigger("slack-notifications-warning", timeoutInSeconds: 100)]
        public Task ProcessWarningMessage(SlackNotificationRequestMsg msg)
        {
            return ProcessMessageFromQueue(msg, "slack-notifications-warning");
        }

        private async Task ProcessMessageFromQueue(SlackNotificationRequestMsg msg, string queueName)
        {
            try
            {
                MuteItem muteItem = await _notificationFilter.GetMutedItem(msg);

                if (muteItem != null && string.IsNullOrEmpty(muteItem.Type))
                {
                    muteItem.MutedMessagesCount++;

                    if (muteItem.MutedMessagesCount > 1)
                        return;

                    await _srvSlackNotifications.SendNotificationAsync(
                        msg.Type,
                        $"*[This message is muted for {muteItem.TimeToMute.TotalMinutes} minute(s)! (using: '{muteItem.Value}')]*: {msg.Message}",
                        $"*[:tired_face: muted :tired_face: ]* {msg.Sender}");
                    return;
                }

                string message = msg.Message;

                if (muteItem?.Type != null && muteItem.Type != msg.Type)
                    message = $"[redirected from {msg.Type}] {message}";

                await _srvSlackNotifications.SendNotificationAsync(muteItem?.Type ?? msg.Type, message, msg.Sender);
            }
            catch (Exception ex)
            {
                _log.WriteError($"ProcessMessageFromQueue: {queueName}", msg.ToJson(), ex);
                throw;
            }
        }
    }
}
