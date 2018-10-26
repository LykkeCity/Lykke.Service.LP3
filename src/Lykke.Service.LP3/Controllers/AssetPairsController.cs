using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Assets;
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

        [HttpGet("{assetPairId}")]
        public Task<AssetPairInfoModel> GetAsync([FromRoute] string assetPairId)
        {
            return Task.FromResult(Mapper.Map<AssetPairInfoModel>(_assetsService.GetAssetPairInfo(assetPairId)));
        }
    }
}
