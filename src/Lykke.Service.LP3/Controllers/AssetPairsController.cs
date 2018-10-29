using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Assets;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class AssetPairsController : IAssetPairsApi
    {
        private readonly IAssetsService _assetsService;

        public AssetPairsController(IAssetsService assetsService)
        {
            _assetsService = assetsService;
        }

        [HttpGet]
        public async Task<IReadOnlyList<AssetPairInfoModel>> GetAllAsync()
        {
            return Mapper.Map<List<AssetPairInfoModel>>(await _assetsService.GetAllAsync());
        }
        
        [HttpGet("{assetPairId}")]
        public Task<AssetPairInfoModel> GetAsync([FromRoute] string assetPairId)
        {
            return Task.FromResult(Mapper.Map<AssetPairInfoModel>(_assetsService.GetAssetPairInfo(assetPairId)));
        }

        [HttpPost]
        public async Task AddAsync([FromBody] AssetPairInfoModel model)
        {
            var entity = Mapper.Map<AssetPairInfo>(model);
            await _assetsService.AddAssetPairInfoAsync(entity);
        }

        [HttpPut]
        public async Task UpdateAsync([FromBody] AssetPairInfoModel model)
        {
            var entity = Mapper.Map<AssetPairInfo>(model);
            await _assetsService.UpdateAssetPairInfoAsync(entity);
        }

        [HttpDelete("{assetPairId}")]
        public async Task DeleteAsync([FromRoute] string assetPairId)
        {
            await _assetsService.DeleteAssetPairInfoAsync(assetPairId);
        }
    }
}
