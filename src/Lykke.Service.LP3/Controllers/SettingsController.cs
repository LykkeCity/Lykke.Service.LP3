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
        
        [HttpGet("baseAssetPair")]
        [ProducesResponseType(typeof(AssetPairSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<AssetPairSettingsModel> GetBaseAssetPairSettingsAsync()
        {
            return Mapper.Map<AssetPairSettingsModel>(await _settingsService.GetBaseAssetPairSettings());
        }
        
        [HttpPost("baseAssetPair")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task SaveBaseAssetPairSettingsAsync([FromBody] AssetPairSettingsModel model)
        {
            await _settingsService.SaveBaseAssetPairSettings(Mapper.Map<AssetPairSettings>(model));
        }
        
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
            await _settingsService.UpdateDependentAssetPairSettingsAsync(Mapper.Map<AssetPairSettings>(model));
        }
        
        [HttpDelete("dependentAssetPairs")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task DeleteDependentAssetPairSettingsAsync(string assetPairId)
        {
            await _settingsService.DeleteDependentAssetPairSettingsAsync(assetPairId);
        }
        
        [HttpGet("levels")]
        [ProducesResponseType(typeof(IReadOnlyList<LevelSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyList<LevelSettingsModel>> GetLevelsSettingsAsync()
        {
            return Mapper.Map<IReadOnlyList<LevelSettingsModel>>(_levelsService.GetLevels());
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
    }
}
