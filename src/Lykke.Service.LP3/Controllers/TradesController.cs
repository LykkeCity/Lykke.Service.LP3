using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Trades;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class TradesController : Controller, ITradesApi
    {
        private readonly ILykkeTradeService _tradeService;

        public TradesController(ILykkeTradeService tradeService)
        {
            _tradeService = tradeService;
        }

        /// <response code="200">A collection of trades.</response>
        [HttpGet]
        [SwaggerOperation("TradesGet")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetAsync(DateTime startDate, DateTime endDate)
        {
            IReadOnlyList<Trade> trades = await _tradeService.GetAsync(startDate, endDate);

            var model = Mapper.Map<List<TradeModel>>(trades);

            return model;
        }

        /// <response code="200">A collection of trades.</response>
        [HttpGet("csv")]
        [SwaggerOperation("TradesGetCsv")]
        [ProducesResponseType(typeof(FileResult), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetCsvAsync(DateTime startDate, DateTime endDate)
        {
            IReadOnlyList<Trade> trades = await _tradeService.GetAsync(startDate, endDate);

            byte[] content;

            const string separator = ",";

            using (TextWriter writer = new StringWriter())
            {
                var headers = new[]
                {
                    "Id",
                    "LimitOrderId",
                    "ExchangeOrderId",
                    "AssetPairId",
                    "TradeType",
                    "Time",
                    "Price",
                    "Volume",
                    "RemainingVolume",
                    "Status",
                    "OppositeSideVolume",
                    "OppositeClientId",
                    "OppositeLimitOrderId"
                };

                writer.WriteLine(string.Join(separator, headers));

                foreach (Trade trade in trades)
                {
                    var values = new[]
                    {
                        trade.Id,
                        trade.LimitOrderId,
                        trade.ExchangeOrderId,
                        trade.AssetPairId,
                        trade.Type.ToString(),
                        trade.Time.ToString("s"),
                        trade.Price.ToString(CultureInfo.InvariantCulture),
                        trade.Volume.ToString(CultureInfo.InvariantCulture),
                        trade.RemainingVolume.ToString(CultureInfo.InvariantCulture),
                        trade.Status.ToString(),
                        trade.OppositeSideVolume.ToString(CultureInfo.InvariantCulture),
                        trade.OppositeClientId,
                        trade.OppositeLimitOrderId
                    };

                    writer.WriteLine(string.Join(separator, values));
                }

                content = Encoding.UTF8.GetBytes(writer.ToString());
            }

            return File(content, "text/csv", $"{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_trades.csv");
        }
    }
}
