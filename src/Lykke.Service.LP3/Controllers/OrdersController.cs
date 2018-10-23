using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Orders;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class OrdersController : Controller, IOrdersApi
    {
        private readonly ILp3Service _lp3Service;

        public OrdersController(ILp3Service lp3Service)
        {
            _lp3Service = lp3Service;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<LimitOrderModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<LimitOrderModel>> GetAllOrdersAsync()
        {
            var orders = await _lp3Service.GetAllOrdersAsync();

            var models = Mapper.Map<List<LimitOrderModel>>(orders);

            return models;
        }
        
        [HttpGet("{assetPairId}")]
        [ProducesResponseType(typeof(IReadOnlyList<LimitOrderModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<LimitOrderModel>> GetOrdersAsync([FromRoute] string assetPairId)
        {
            var orders = await _lp3Service.GetOrdersForAssetAsync(assetPairId);

            var models = Mapper.Map<List<LimitOrderModel>>(orders);

            return models;
        }

        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public Task CreateOrderAsync([FromBody] LimitOrderModel orderModel)
        {
            var limitOrder = Mapper.Map<LimitOrder>(orderModel);

            return _lp3Service.AddOrderAsync(limitOrder);
        }

        [HttpDelete("{assetPairId}/{orderId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public Task CancelOrderAsync([FromRoute] string assetPairId, [FromRoute] Guid orderId)
        {
            return _lp3Service.CancelOrderAsync(assetPairId, orderId);
        }

        [HttpDelete("{assetPairId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public Task CancelAllOrdersAsync(string assetPairId)
        {
            return _lp3Service.CancelAllOrdersAsync(assetPairId);
        }

        [HttpPost("{assetPairId}/{orderId}/recreate")]
        public async Task<LimitOrderModel> RecreateOrderAsync([FromRoute] string assetPairId, [FromRoute] Guid orderId)
        {
            var newOrder = await _lp3Service.RecreateOrderAsync(assetPairId, orderId);

            return Mapper.Map<LimitOrderModel>(newOrder);
        }
    }
}
