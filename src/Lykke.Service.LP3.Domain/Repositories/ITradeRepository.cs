using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ITradeRepository
    {
        Task<IReadOnlyList<Trade>> GetAsync(DateTime startDate, DateTime endDate);

        Task InsertAsync(Trade trade);
    }
}
