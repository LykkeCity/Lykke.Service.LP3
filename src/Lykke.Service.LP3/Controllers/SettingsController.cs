using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Settings;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        
        /// <response code="200">Levels settings.</response>
        [HttpGet("levels")]
        [ProducesResponseType(typeof(IReadOnlyList<LevelSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<LevelSettingsModel>> GetLevelsSettingsAsync()
        {
            var levelSettings = await _settingsService.GetLevelSettingsAsync();
            
            var model = Mapper.Map<IReadOnlyList<LevelSettingsModel>>(levelSettings);

            return model;
        }
//        
//        /// <response code="204">A level settings successfully added.</response>
//        [HttpPost]
//        //[SwaggerOperation("LevelSettingsSave")]
//        [ProducesResponseType((int)HttpStatusCode.NoContent)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
//        public async Task AddAsync([FromBody] AssetHedgeSettingsModel model, string clientId)
//        {
//            if (string.IsNullOrEmpty(clientId))
//            {
//                throw new ValidationApiException($"{nameof(clientId)} required");
//            }
//
//            var exchange = (await _settingsService.GetExternalExchangesAsync())
//                .FirstOrDefault(e => e.HasApi && string.Equals(e.Name, model.Exchange, StringComparison.InvariantCultureIgnoreCase));
//            if (exchange == null)
//            {
//                throw new ValidationApiException("Hedging is not allowed for the specified exchange");
//            }
//
//            AssetHedgeSettings existingAssetSettings = (await _assetHedgeSettingsService.GetAsync(model.Asset));
//            if (existingAssetSettings != null)
//            {
//                throw new ValidationApiException("Hedging settings already exist for the specified asset");
//            }
//
//            var assetHedgeSettings = Mapper.Map<AssetHedgeSettings>(model);
//
//            await _assetHedgeSettingsService.AddAsync(assetHedgeSettings, clientId);
//        }
    }
}
