using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILykkeTradeService
    {
        Task<IReadOnlyList<Trade>> GetAsync(DateTime startDate, DateTime endDate);
        Task HandleAsync(IReadOnlyCollection<Trade> trades);
    }
}
