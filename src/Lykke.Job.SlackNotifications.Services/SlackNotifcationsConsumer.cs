using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Job.SlackNotifications.Core;

namespace Lykke.Job.SlackNotifications.Services
{
    public class SlackNotifcationsConsumer
    {
        private readonly SrvSlackNotifications _srvSlackNotifications;
        private readonly ILog _log;

        public SlackNotifcationsConsumer(SrvSlackNotifications srvSlackNotifications, ILog log)
        {
            _srvSlackNotifications = srvSlackNotifications;
            _log = log;
        }

        [QueueTrigger("slack-notifications")]
        public async Task ProcessInMessage(SlackNotificationRequestMsg msg)
        {
            try
            {
                await _srvSlackNotifications.SendNotification(msg.Type, msg.Message, msg.Sender);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SlackNotificationRequestsConsumer", "ProcessInMessage", msg.ToJson(), ex);
                throw;
            }
        }
    }
}
