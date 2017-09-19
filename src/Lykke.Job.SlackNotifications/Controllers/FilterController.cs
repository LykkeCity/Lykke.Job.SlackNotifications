using Microsoft.AspNetCore.Mvc;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Controllers
{
    public class FilterController : Controller
    {
        private readonly INotificationFilter _notificationFilter;

        public FilterController(INotificationFilter notificationFilter)
        {
            _notificationFilter = notificationFilter;
        }

        [HttpPost]
        [Route("api/[controller]/Mute/{sender}")]
        public void Mute(string sender)
        {
            _notificationFilter.MuteSender(sender);
        }

        [HttpPost]
        [Route("api/[controller]/Unmute/{sender}")]
        public void Unmute(string sender)
        {
            _notificationFilter.UnmuteSender(sender);
        }
    }
}
