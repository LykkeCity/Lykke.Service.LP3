using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    public class LykkeExchange : ILykkeExchange, IStartable
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly ISettingsService _settingsService;
        private readonly IAssetPairsReadModelRepository _assetPairsService;
        private readonly IAssetsReadModelRepository _assetsService;
        private readonly IOrderIdsMappingRepository _orderIdsMappingRepository;
        private readonly ILog _log;

        private readonly Dictionary<string, Dictionary<Guid, string>> _idsMap = 
            new Dictionary<string, Dictionary<Guid, string>>();
        
        public LykkeExchange(ILogFactory logFactory,
            IMatchingEngineClient matchingEngineClient,
            ISettingsService settingsService,
            IAssetPairsReadModelRepository assetPairsService,
            IAssetsReadModelRepository assetsService,
            IOrderIdsMappingRepository orderIdsMappingRepository)
        {
            _matchingEngineClient = matchingEngineClient;
            _settingsService = settingsService;
            _assetPairsService = assetPairsService;
            _assetsService = assetsService;
            _orderIdsMappingRepository = orderIdsMappingRepository;

            _log = logFactory.CreateLog(this);
        }
        
        public async Task ApplyAsync(string assetPairId, IReadOnlyList<LimitOrder> limitOrders)
        {
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                throw new Exception("WalletId is not set");

            AssetPair assetPair = _assetPairsService.TryGetIfEnabled(assetPairId);
            if (assetPair == null)
            {
                throw new Exception($"AssetService have returned null for asset pair {assetPairId}");
            }

            Asset baseAsset = _assetsService.TryGet(assetPair.BaseAssetId);
            if (baseAsset == null)
            {
                throw new Exception(
                    $"AssetService have returned null for base asset {assetPair.BaseAssetId} from pair {assetPairId}");
            }

            var mapInternalToExternal = new Dictionary<Guid, string>();
            
            var multiOrderItems = new List<MultiOrderItemModel>();

            foreach (LimitOrder limitOrder in limitOrders)
            {
                decimal roundedVolume = Math.Round(Math.Abs(limitOrder.Volume), baseAsset.Accuracy);
                
                if (roundedVolume < assetPair.MinVolume)
                {
                    _log.Warning("Order volume less then minimal", context: $"order: {limitOrder.ToJson()}, " +
                                            $"rounded volume: {roundedVolume}, min volume: {assetPair.MinVolume}");

                    limitOrder.Error = LimitOrderError.TooSmallVolume;
                    limitOrder.ErrorMessage = $"Minimal volume for {assetPairId} is {assetPair.MinVolume}";
                    continue;
                }

                var multiOrderItem = new MultiOrderItemModel
                {
                    Id = Guid.NewGuid().ToString("D"),
                    OrderAction = limitOrder.TradeType.ToOrderAction(),
                    Price = (double) limitOrder.Price.TruncateDecimalPlaces(assetPair.Accuracy, toUpper: limitOrder.TradeType == TradeType.Sell),
                    Volume = (double) roundedVolume,
                    OldId = limitOrder.Id != default && _idsMap.ContainsKey(assetPairId) && _idsMap[assetPairId].ContainsKey(limitOrder.Id)
                        ? _idsMap[assetPairId][limitOrder.Id]
                        : null
                };

                limitOrder.MultiOrderItemId = multiOrderItem.Id;
                
                multiOrderItems.Add(multiOrderItem);

                if (limitOrder.Id != default)
                {
                    mapInternalToExternal[limitOrder.Id] = multiOrderItem.Id;
                }
            }

            var multiLimitOrder = new MultiLimitOrderModel
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = walletId,
                AssetPairId = assetPair.Id,
                CancelPreviousOrders = true,
                Orders = multiOrderItems,
                CancelMode = CancelMode.BothSides
            };

            _log.Info("ME place multi limit order request", new {request = $"data: {multiLimitOrder.ToJson()}"});

            MultiLimitOrderResponse response;

            try
            {
                response = await _matchingEngineClient.PlaceMultiLimitOrderAsync(multiLimitOrder);
                
                _idsMap[assetPairId] = mapInternalToExternal;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred during creating limit orders",
                    new {request = $"data: {multiLimitOrder.ToJson()}"});

                throw;
            }

            if (response == null)
            {
                throw new Exception("ME response is null");
            }

            if (response.Statuses.All(x => x.Status == MeStatusCodes.Ok))
            {
                _log.Info("ME place multi limit order response", new {response = $"data: {response.ToJson()}"});
            }
            else
            {
                _log.Warning("ME place multi limit order response. Some orders have unsuccessful codes.", context: new {response = $"data: {response.ToJson()}"});
            }

            foreach (var orderStatus in response.Statuses)
            {
                var limitOrder = limitOrders.Single(e => e.MultiOrderItemId == orderStatus.Id);

                limitOrder.Error = orderStatus.Status.ToOrderError();
                limitOrder.ErrorMessage = limitOrder.Error != LimitOrderError.Unknown 
                    ? orderStatus.StatusReason
                    : !string.IsNullOrEmpty(orderStatus.StatusReason) ? orderStatus.StatusReason : "Unknown error";
            }

            await PersistMapping(assetPairId);
        }

        private async Task PersistMapping(string assetPairId)
        {
            try
            {
                await _orderIdsMappingRepository.PersistMapping(assetPairId, _idsMap[assetPairId]);
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on saving order ids mapping");
            }
        }

        public void Start()
        {
            try
            {
                Dictionary<string, Dictionary<Guid, string>> map = _orderIdsMappingRepository.RestoreMapping().GetAwaiter().GetResult();

                foreach (var keyValuePair in map)
                {
                    _idsMap.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on restoring orders mapping. LykkeExchange will start without mapping.");
            }
        }
    }
}
