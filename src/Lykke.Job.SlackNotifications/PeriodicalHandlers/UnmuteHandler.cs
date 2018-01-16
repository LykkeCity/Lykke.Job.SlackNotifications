using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.PeriodicalHandlers
{
    public class UnmuteHandler : TimerPeriod
    {
        private readonly INotificationFilter _notificationFilter;

        public UnmuteHandler(
            INotificationFilter notificationFilter,
            ILog log) :
            base(nameof(UnmuteHandler), (int)TimeSpan.FromMinutes(1).TotalMilliseconds, log)
        {
            _notificationFilter = notificationFilter;
        }

        public override async Task Execute()
        {
            _notificationFilter.UnmuteExpired();
            await Task.CompletedTask;
        }
    }
}
