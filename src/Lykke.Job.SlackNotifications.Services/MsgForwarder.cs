using System.Threading.Tasks;
using AzureStorage.Queue;
using Lykke.Job.SlackNotifications.Core.Services;

namespace Lykke.Job.SlackNotifications.Services
{
    public class MsgForwarder : IMsgForwarder
    {
        private readonly IQueueExt _queueExt;

        public MsgForwarder(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task ForwardMsgAsync(string msg)
        {
            return _queueExt.PutRawMessageAsync(msg);
        }
    }
}
