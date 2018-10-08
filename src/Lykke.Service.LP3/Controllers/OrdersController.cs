using System.Collections.Generic;
using System.Net;
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
        public IReadOnlyList<LimitOrderModel> GetAll()
        {
            var orders = _lp3Service.GetOrders();

            var models = Mapper.Map<List<LimitOrderModel>>(orders);

            return models;
        }
    }
}
