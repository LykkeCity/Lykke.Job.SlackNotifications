using Microsoft.AspNetCore.Mvc;
using Lykke.Job.SlackNotifications.Core.Services;
using Lykke.Job.SlackNotifications.Extensions;
using Lykke.Job.SlackNotifications.Models;

namespace Lykke.Job.SlackNotifications.Controllers
{
    [Route("api/[controller]")]
    public class FilterController : Controller
    {
        private readonly INotificationFilter _notificationFilter;

        public FilterController(INotificationFilter notificationFilter)
        {
            _notificationFilter = notificationFilter;
        }

        [HttpPost]
        [Route("MuteSender")]
        public IActionResult MuteSender([FromBody]MuteModel model)
        {
            if (model == null)
                return BadRequest("Model is not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetError());

            _notificationFilter.MuteSender(model.ToDomain());
            return Ok();
        }

        [HttpPost]
        [Route("UnmuteSender/{sender}")]
        public IActionResult UnmuteSender(string sender)
        {
            _notificationFilter.UnmuteSender(sender);
            return Ok();
        }

        [HttpPost]
        [Route("MuteMessagePrefix")]
        public IActionResult MuteMessagePrefix([FromBody]MuteModel model)
        {
            if (model == null)
                return BadRequest("Model is not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetError());

            _notificationFilter.MuteMessagePrefix(model.ToDomain());
            return Ok();
        }

        [HttpPost]
        [Route("UnmuteMessagePrefix/{prefix}")]
        public IActionResult UnmuteMessagePrefix(string prefix)
        {
            _notificationFilter.UnmuteMessagePrefix(prefix);
            return Ok();
        }
        
        [HttpPost]
        [Route("MuteRegexMessage")]
        public IActionResult MuteRegexMessage([FromBody]MuteModel model)
        {
            if (model == null)
                return BadRequest("Model is not provided");

            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetError());

            _notificationFilter.MuteRegexMessage(model.ToDomain());
            return Ok();
        }

        [HttpPost]
        [Route("UnmuteRegexMessage/{regex}")]
        public IActionResult UnmuteRegexMessage(string regex)
        {
            _notificationFilter.UnmuteRegexMessage(regex);
            return Ok();
        }

        [HttpGet]
        [Route("all")]
        public IActionResult GetAllFilters()
        {
            return Ok(_notificationFilter.GetAllFilters());
        }
    }
}
