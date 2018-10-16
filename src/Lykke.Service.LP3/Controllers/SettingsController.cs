using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Settings;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;
        private readonly ILevelsService _levelsService;

        public SettingsController(ISettingsService settingsService,
            ILevelsService levelsService)
        {
            _settingsService = settingsService;
            _levelsService = levelsService;
        }

        [HttpGet("walletId")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_settingsService.GetWalletId());
        }

        [HttpGet("availableExchanges")]
        [ProducesResponseType(typeof(IReadOnlyList<string>), (int)HttpStatusCode.OK)]
        public Task<IReadOnlyList<string>> GetAvailableExchangesAsync()
        {
            return Task.FromResult(_settingsService.GetAvailableExternalExchanges());
        }

        #region BaseAssetPair

        [HttpGet("baseAssetPair")]
        [ProducesResponseType(typeof(BaseAssetPairSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<BaseAssetPairSettingsModel> GetBaseAssetPairSettingsAsync()
        {
            return Mapper.Map<BaseAssetPairSettingsModel>(await _settingsService.GetBaseAssetPairSettingsAsync());
        }
        
        [HttpPost("baseAssetPair")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task SaveBaseAssetPairSettingsAsync([FromBody] BaseAssetPairSettingsModel model)
        {
            await _settingsService.SaveBaseAssetPairSettingsAsync(Mapper.Map<AssetPairSettings>(model));
        }

        [HttpDelete("baseAssetPair")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task DeleteBaseAssetPairSettingsAsync()
        {
            await _settingsService.DeleteBaseAssetPairSettingsAsync();
        }

        #endregion
        
        
        #region DependentAssetPairSettings
        
        [HttpGet("dependentAssetPairs")]
        [ProducesResponseType(typeof(IReadOnlyList<AssetPairSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<AssetPairSettingsModel>> GetDependentAssetPairSettingsAsync()
        {
            return Mapper.Map<List<AssetPairSettingsModel>>(await _settingsService.GetDependentAssetPairsSettingsAsync());
        }
        
        [HttpPost("dependentAssetPairs")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task UpdateDependentAssetPairSettingsAsync([FromBody] AssetPairSettingsModel model)
        {
            if (!string.Equals(model.CrossInstrumentSource, Consts.LykkeExchangeName, StringComparison.InvariantCultureIgnoreCase) &&
                !_settingsService.GetAvailableExternalExchanges().Any(x =>
                    string.Equals(x, model.CrossInstrumentSource, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ValidationApiException($"For adding cross instrument on {model.CrossInstrumentSource} exchange " +
                                                 "it's needed to add the exchange adapter in the service global settings. " +
                                                 "Use GET /api/settings/availableExchagnes to see the list.");
            }
            
            
            await _settingsService.UpdateDependentAssetPairSettingsAsync(Mapper.Map<AssetPairSettings>(model));
        }
        
        [HttpDelete("dependentAssetPairs")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task DeleteDependentAssetPairSettingsAsync(string assetPairId)
        {
            await _settingsService.DeleteDependentAssetPairSettingsAsync(assetPairId);
        }
        
        #endregion


        #region Levels

        [HttpGet("levels")]
        [ProducesResponseType(typeof(IReadOnlyList<LevelSettingsModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyList<LevelSettingsModel>> GetLevelsSettingsAsync()
        {
            return Task.FromResult(Mapper.Map<IReadOnlyList<LevelSettingsModel>>(_levelsService.GetLevels()));
        }
        
        [HttpPost("levels")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task AddAsync([FromBody] LevelSettingsModel model)
        {
            var levels = _levelsService.GetLevels();
            if (levels.Any(x => string.Equals(x.Name, model.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ValidationApiException($"A level with name {model.Name} already exist");
            }

            await _levelsService.AddAsync(Mapper.Map<Level>(model));
        }

        [HttpDelete("levels/{name}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task DeleteAsync(string name)
        {
            await _levelsService.DeleteAsync(name);
        }
        
        [HttpPut("levels")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task UpdateAsync([FromBody] LevelSettingsModel model)
        {
            var level = _levelsService.GetLevels();
            if (level.All(x => !string.Equals(x.Name, model.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new ValidationApiException($"A level with name {model.Name} doesn't exist");
            }

            await _levelsService.UpdateAsync(model.Name, model.Delta, model.Volume);
        }

        #endregion


        #region AdditionalVolumeSettings
        
        [HttpGet("additionalVolumeSettings")]
        [ProducesResponseType(typeof(AdditionalVolumeSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<AdditionalVolumeSettingsModel> GetAdditionalVolumeSettingsAsync()
        {
            return Mapper.Map<AdditionalVolumeSettingsModel>(await _settingsService.GetAdditionalVolumeSettingsAsync());
        }

        [HttpPost("additionalVolumeSettings")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task UpdateAdditionalVolumeSettingsAsync([FromBody] AdditionalVolumeSettingsModel model)
        {
            await _settingsService.UpdateAdditionalVolumeSettingsAsync(Mapper.Map<AdditionalVolumeSettings>(model));
        }

        #endregion
    }
}
