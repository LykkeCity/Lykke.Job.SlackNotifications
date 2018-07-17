using System.Threading.Tasks;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class MsgForwarderStub : IMsgForwarder
    {
        public Task ForwardMsgAsync(string msg)
        {
            //Do nothing
            return Task.CompletedTask;
        }
    }
}
