using System.Threading.Tasks;
using Autofac;
using Common;
//using Lykke.Job.LP3.Contract;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IMyRabbitPublisher : IStartable, IStopable
    {
        //Task PublishAsync(MyPublishedMessage message);
    }
}
