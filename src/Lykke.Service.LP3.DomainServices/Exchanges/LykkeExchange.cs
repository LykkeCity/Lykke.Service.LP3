using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    public class LykkeExchange : ILykkeExchange
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly ISettingsService _settingsService;
        private readonly IAssetPairsReadModelRepository _assetsService;
        private readonly ILog _log;

        private readonly ConcurrentDictionary<string, List<LimitOrder>> _orders = 
            new ConcurrentDictionary<string, List<LimitOrder>>();
        
        private readonly Dictionary<string, Dictionary<Guid, string>> _idsMap = 
            new Dictionary<string, Dictionary<Guid, string>>(); // TODO: persist this map
        
        public LykkeExchange(ILogFactory logFactory,
            IMatchingEngineClient matchingEngineClient,
            ISettingsService settingsService,
            IAssetPairsReadModelRepository assetsService)
        {
            _matchingEngineClient = matchingEngineClient;
            _settingsService = settingsService;
            _assetsService = assetsService;

            _log = logFactory.CreateLog(this);
        }
        
        public async Task ApplyAsync(string assetPairId, IReadOnlyList<LimitOrder> limitOrders)
        {
            if (_orders.TryGetValue(assetPairId, out var orders) && orders != null && limitOrders.SequenceEqual(orders, new LimitOrdersComparer()))
            {
                _log.Info("New orders are the same as previously placed, don't replace");
                return;
            }
            
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                throw new Exception("WalletId is not set");

            AssetPair assetPair = _assetsService.TryGetIfEnabled(assetPairId);
            if (assetPair == null)
            {
                throw new Exception($"AssetService have returned null for asset pair {assetPairId}");
            }

            var mapExternalToInternal = new Dictionary<string, Guid>();
            var mapInternalToExternal = new Dictionary<Guid, string>();
            
            var multiOrderItems = new List<MultiOrderItemModel>();

            foreach (LimitOrder limitOrder in limitOrders)
            {
                var multiOrderItem = new MultiOrderItemModel
                {
                    Id = Guid.NewGuid().ToString("D"),
                    OrderAction = limitOrder.TradeType.ToOrderAction(),
                    Price = (double) limitOrder.Price.TruncateDecimalPlaces(assetPair.Accuracy, toUpper: limitOrder.TradeType == TradeType.Sell),
                    Volume = (double) Math.Round(Math.Abs(limitOrder.Volume), assetPair.InvertedAccuracy),
                    OldId = _idsMap.ContainsKey(assetPairId) && _idsMap[assetPairId].ContainsKey(limitOrder.Id) ? _idsMap[assetPairId][limitOrder.Id] : null
                };

                multiOrderItems.Add(multiOrderItem);

                mapExternalToInternal[multiOrderItem.Id] = limitOrder.Id;
                mapInternalToExternal[limitOrder.Id] = multiOrderItem.Id;
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
                _orders[assetPairId] = limitOrders.ToList();
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
                if (mapExternalToInternal.TryGetValue(orderStatus.Id, out var limitOrderId))
                {
                    var limitOrder = limitOrders.Single(e => e.Id == limitOrderId);

                    limitOrder.Error = orderStatus.Status.ToOrderError();
                    limitOrder.ErrorMessage = limitOrder.Error != LimitOrderError.Unknown 
                        ? orderStatus.StatusReason
                        : !string.IsNullOrEmpty(orderStatus.StatusReason) ? orderStatus.StatusReason : "Unknown error";
                }
            }
        }
    }
}
