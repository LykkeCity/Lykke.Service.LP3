using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class LimitOrderService : ILimitOrderService
    {
        private readonly ILimitOrderRepository _limitOrderRepository;

        public LimitOrderService(ILimitOrderRepository limitOrderRepository)
        {
            _limitOrderRepository = limitOrderRepository;
        }
    }
}
