using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class InitialPriceController : Controller, IInitialPriceApi
    {
        private readonly IInitialPriceService _initialPriceService;

        public InitialPriceController(IInitialPriceService initialPriceService)
        {
            _initialPriceService = initialPriceService;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(InitialPriceModel), (int) HttpStatusCode.OK)]
        public async Task<InitialPriceModel> GetAsync()
        {
            var entity = await _initialPriceService.GetAsync();
            
            var model = Mapper.Map<InitialPriceModel>(entity);

            return model;
        }
        
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task AddOrUpdateAsync([FromBody] InitialPriceModel model)
        {
            await _initialPriceService.AddOrUpdateAsync(model.Price);
        }

        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task DeleteAsync()
        {
            await _initialPriceService.DeleteAsync();
        }
    }
}
