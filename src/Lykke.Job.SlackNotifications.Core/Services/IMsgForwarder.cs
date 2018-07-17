using System.Threading.Tasks;

namespace Lykke.Job.SlackNotifications.Core.Services
{
    public interface IMsgForwarder
    {
        Task ForwardMsg(string msg);
    }
}
