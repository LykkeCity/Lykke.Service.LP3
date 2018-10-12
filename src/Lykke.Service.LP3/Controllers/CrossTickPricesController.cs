using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("api/[controller]")]
    public class CrossTickPricesController : Controller, ICrossTickPricesApi
    {
        private readonly ICrossRateService _crossRateService;

        public CrossTickPricesController(ICrossRateService crossRateService)
        {
            _crossRateService = crossRateService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TickPriceModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TickPriceModel>> GetAllAsync()
        {
            var tickPrices = await _crossRateService.GetAllTickPricesAsync();

            var models = Mapper.Map<List<TickPriceModel>>(tickPrices);

            return models;
        }
    }
}
