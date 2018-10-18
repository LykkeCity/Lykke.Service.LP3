using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Orders;
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
        public Task<IReadOnlyList<LimitOrderModel>> GetOrdersAsync(string assetPairId)
        {
            var orders = _lp3Service.GetOrders();

            if (!string.IsNullOrEmpty(assetPairId))
            {
                orders = orders.Where(x => string.Equals(x.AssetPairId, assetPairId, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var models = Mapper.Map<List<LimitOrderModel>>(orders);

            return Task.FromResult((IReadOnlyList<LimitOrderModel>)models);
        }
    }
}
