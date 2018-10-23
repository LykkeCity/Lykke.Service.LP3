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
    public class OrderBookTradersController : Controller, IOrderBookTradersApi
    {
        private readonly IOrderBookTraderService _orderBookTraderService;
        private readonly ILp3Service _lp3Service;

        public OrderBookTradersController(IOrderBookTraderService orderBookTraderService,
            ILp3Service lp3Service)
        {
            _orderBookTraderService = orderBookTraderService;
            _lp3Service = lp3Service;
        }


        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<OrderBookTraderModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<OrderBookTraderModel>> GetAllAsync()
        {
            var traders = await _orderBookTraderService.GetOrderBookTradersAsync();
            var models = Mapper.Map<List<OrderBookTraderModel>>(traders);
            return models;
        }

        [HttpGet("{assetPairId}")]
        [ProducesResponseType(typeof(OrderBookTraderModel), (int) HttpStatusCode.OK)]
        public async Task<OrderBookTraderModel> GetAsync(string assetPairId)
        {
            var trader = await _orderBookTraderService.GetOrderBookTraderAsync(assetPairId);
            var model = Mapper.Map<OrderBookTraderModel>(trader);
            return model;
        }

        [HttpPost("{assetPairId}/softStop")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public Task SoftStopAsync(string assetPairId)
        {
            return _orderBookTraderService.SoftStopAsync(assetPairId);
        }

        [HttpPost("{assetPairId}/softStart")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public Task SoftStartAsync(string assetPairId)
        {
            return _orderBookTraderService.SoftStartAsync(assetPairId);
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
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task DeleteOrderBookTraderAsync([FromRoute] string assetPairId)
        {
            await _lp3Service.DeleteOrderBookAsync(assetPairId);
        }
    }
}
