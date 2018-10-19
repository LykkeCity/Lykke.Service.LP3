using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Balances.AutorestClient.Models;
using Lykke.Service.Balances.Client;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices.Balances
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService, IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly IAssetLinkService _assetLinkService;
        private readonly IBalancesClient _balancesClient;
        private readonly InMemoryCache<Balance> _cache;
        private readonly ILog _log;

        public BalanceService(
            ISettingsService settingsService,
            IAssetLinkService assetLinkService,
            IBalancesClient balancesClient,
            ILogFactory logFactory)
        {
            _cache = new InMemoryCache<Balance>(balance => balance.AssetId, true);

            _settingsService = settingsService;
            _assetLinkService = assetLinkService;
            _balancesClient = balancesClient;
            _log = logFactory.CreateLog(this);
        }

        public Task<IReadOnlyCollection<Balance>> GetAllAsync()
        {
            IReadOnlyCollection<Balance> balances = _cache.GetAll() ?? new Balance[0];

            return Task.FromResult(balances);
        }

        public Task<Balance> GetByAssetIdAsync(string assetId)
        {
            return Task.FromResult(_cache.Get(assetId) ?? new Balance(assetId, decimal.Zero, decimal.Zero));
        }

        public async Task UpdateBalancesAsync()
        {
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                return;

            try
            {
                IEnumerable<ClientBalanceResponseModel> exchangeBalances =
                    await _balancesClient.GetClientBalances(walletId);

                IDictionary<string, AssetLink> assetMap = (await _assetLinkService.GetAllAsync())
                    .ToDictionary(o => o.ExternalAssetId, o => o);

                IReadOnlyCollection<Balance> balances = exchangeBalances
                    .Select(exchangeBalance =>
                    {
                        string assetId = assetMap.TryGetValue(exchangeBalance.AssetId, out AssetLink assetLink)
                            ? assetLink.AssetId
                            : exchangeBalance.AssetId;

                        return new Balance(assetId, exchangeBalance.Balance, exchangeBalance.Reserved);
                    })
                    .ToArray();

                _cache.Set(balances);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred while getting balances from Lykke exchange.");
            }
        }

        public void Start()
        {
            _log.Info("Updating balances on start");
            UpdateBalancesAsync().GetAwaiter().GetResult();
        }
    }
}
