using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Client.Models.Balances;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class BalancesController : Controller, IBalancesApi
    {
        private readonly IBalanceService _balanceService;
        private readonly ISettingsService _settingsService;

        public BalancesController(IBalanceService balanceService, ISettingsService settingsService)
        {
            _balanceService = balanceService;
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of balances.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<AssetBalanceModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<AssetBalanceModel>> GetAllAsync()
        {
            IReadOnlyCollection<Balance> balances = await _balanceService.GetAllAsync();

            var model = Mapper.Map<IReadOnlyCollection<AssetBalanceModel>>(balances);
            return model;
        }

        /// <inheritdoc/>
        /// <response code="200">The balance of asset.</response>
        [HttpGet("{assetId}")]
        [ProducesResponseType(typeof(AssetBalanceModel), (int)HttpStatusCode.OK)]
        public async Task<AssetBalanceModel> GetByAssetIdAsync(string assetId)
        {
            Balance balance = await _balanceService.GetByAssetIdAsync(assetId);

            var model = Mapper.Map<AssetBalanceModel>(balance);
            return model;
        }
    }
}
