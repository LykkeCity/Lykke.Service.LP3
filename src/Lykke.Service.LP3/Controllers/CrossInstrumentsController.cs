using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.CrossInstruments;
using Lykke.Service.LP3.Domain.CrossInstruments;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class CrossInstrumentsController : Controller, ICrossInstrumentsApi
    {
        private readonly ICrossInstrumentService _crossInstrumentService;
        
        public CrossInstrumentsController(ICrossInstrumentService crossInstrumentService)
        {
            _crossInstrumentService = crossInstrumentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CrossInstrumentModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<CrossInstrumentModel>> GetAsync()
        {
            var crossInstruments = await _crossInstrumentService.GetAsync();
            return Mapper.Map<List<CrossInstrumentModel>>(crossInstruments);
        }

        [HttpPost]
        public async Task AddAsync([FromBody] CrossInstrumentModel model)
        {
            if (await _crossInstrumentService.GetAsync(model.Exchange, model.AssetPairId) != null)
            {
                throw new ValidationApiException($"A cross instrument {model.AssetPairId}@{model.Exchange} already exists");
            }
            
            var crossInstrument = Mapper.Map<CrossInstrument>(model);
            await _crossInstrumentService.AddAsync(crossInstrument);
        }

        [HttpPut]
        public async Task UpdateAsync([FromBody] CrossInstrumentModel model)
        {
            if (await _crossInstrumentService.GetAsync(model.Exchange, model.AssetPairId) == null)
            {
                throw new ValidationApiException($"A cross instrument {model.AssetPairId}@{model.Exchange} doesn't exist");
            }
            
            var crossInstrument = Mapper.Map<CrossInstrument>(model);
            await _crossInstrumentService.UpdateAsync(crossInstrument);
        }

        [HttpDelete]
        public async Task DeleteAsync(string exchange, string assetPairId)
        {
            await _crossInstrumentService.DeleteAsync(exchange, assetPairId);
        }
    }
}
