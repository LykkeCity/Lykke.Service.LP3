using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models;
using Lykke.Service.LP3.Client.Models.Settings;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;
        private readonly IOrderBookTraderService _orderBookTraderService;
        private readonly ILp3Service _lp3Service;

        public SettingsController(ISettingsService settingsService,
            IOrderBookTraderService orderBookTraderService,
            ILp3Service lp3Service)
        {
            _settingsService = settingsService;
            _orderBookTraderService = orderBookTraderService;
            _lp3Service = lp3Service;
        }

        [HttpGet("walletId")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_settingsService.GetWalletId());
        }
        
        [HttpGet("orderBookTraders")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderBookTraderModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<OrderBookTraderModel>> GetOrderBookTradersSettingsAsync()
        {
            return Mapper.Map<List<OrderBookTraderModel>>(await _orderBookTraderService.GetOrderBookTradersAsync());
        }
        
        [HttpPost("orderBookTraders")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task AddOrderBookTraderAsync([FromBody] OrderBookTraderSettingsModel model)
        {
            var existingSettings = await _orderBookTraderService.GetOrderBookTradersAsync();
            
            if (existingSettings.Any(x => string.Equals(x.AssetPairId, model.AssetPairId, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ValidationApiException($"OrderBookTrader for asset pair {model.AssetPairId} already exists.");
            }

            await _lp3Service.AddOrderBookTraderAsync(Mapper.Map<OrderBookTraderSettings>(model));
        }

        [HttpPut("orderBookTraders")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task UpdateOrderBookTraderSettingsAsync([FromBody] OrderBookTraderSettingsModel model)
        {
            var existingSettings = await _orderBookTraderService.GetOrderBookTradersAsync();
            
            if (!existingSettings.Any(x => string.Equals(x.AssetPairId, model.AssetPairId, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ValidationApiException($"DependentAssetPairSettings for asset pair {model.AssetPairId} doesn't exists.");
            }
         
            await _lp3Service.UpdateOrderBookTraderSettingsAsync(Mapper.Map<OrderBookTraderSettings>(model));
        }
        
        [HttpDelete("orderBookTraders/{assetPairId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task DeleteOrderBookTraderAsync([FromRoute] string assetPairId)
        {
            await _lp3Service.DeleteOrderBookAsync(assetPairId);
        }
    }
}
