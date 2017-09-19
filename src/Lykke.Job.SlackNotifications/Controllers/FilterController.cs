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
        [Route("api/[controller]/MuteSender/{sender}")]
        public void MuteSender(string sender)
        {
            _notificationFilter.MuteSender(sender);
        }

        [HttpPost]
        [Route("api/[controller]/UnmuteSender/{sender}")]
        public void UnmuteSender(string sender)
        {
            _notificationFilter.UnmuteSender(sender);
        }

        [HttpPost]
        [Route("api/[controller]/MuteMessagePrefix/{prefix}")]
        public void MuteMessagePrefix(string prefix)
        {
            _notificationFilter.MuteMessagePrefix(prefix);
        }

        [HttpPost]
        [Route("api/[controller]/UnmuteMessagePrefix/{prefix}")]
        public void UnmuteMessagePrefix(string prefix)
        {
            _notificationFilter.UnmuteMessagePrefix(prefix);
        }
    }
}
