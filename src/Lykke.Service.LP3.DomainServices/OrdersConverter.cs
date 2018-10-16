using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrdersConverter : IOrdersConverter
    {
        private readonly ICrossRateService _crossRateService;
        private readonly IAssetsService _assetsService;
        private readonly ILog _log;
        
        private readonly OrdersConverterLogic _conversionLogic = new OrdersConverterLogic();

        public OrdersConverter(
            ILogFactory logFactory,
            ICrossRateService crossRateService,
            IAssetsService assetsService)
        {
            _crossRateService = crossRateService;
            _assetsService = assetsService;
            _log = logFactory.CreateLog(this);
        }
        
        public async Task<IReadOnlyList<DependentLimitOrder>> ConvertAsync(IReadOnlyList<LimitOrder> orders, AssetPairSettings assetPairSettings)
        {
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            if (assetPairSettings == null) throw new ArgumentNullException(nameof(assetPairSettings));
            
            var crossTickPrice = await _crossRateService
                .GetLastTickPriceAsync(assetPairSettings.CrossInstrumentSource, assetPairSettings.CrossInstrumentAssetPair);

            if (crossTickPrice == null)
            {
                throw new Exception($"No tick price for convert to {assetPairSettings.AssetPairId}");
            }

            var assetPairInfo = _assetsService.GetAssetPairInfo(assetPairSettings.AssetPairId);
            if (assetPairInfo == null)
            {
                throw new Exception($"No asset pair info for {assetPairSettings.AssetPairId}");
            }

            var result = orders.Select(order => _conversionLogic.Convert(order, assetPairSettings, crossTickPrice, assetPairInfo)).ToList();

            _log.Info("Orders were converted", context: 
                $"from: [{string.Join(", ", orders.Select(x => x.ToJson()))}], " +
                $"to: [{string.Join(", ", result.Select(x => x.ToJson()))}]");
            
            return result;
        }
    }
}
