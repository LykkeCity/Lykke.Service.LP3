using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Trades;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ITradesApi
    {
        /// <summary>
        /// Get the executed trades by time interval
        /// </summary>
        [Get("/api/trades")]
        Task<IReadOnlyList<TradeModel>> GetAsync([Query] DateTime startDate, [Query] DateTime endDate);
    }
}
