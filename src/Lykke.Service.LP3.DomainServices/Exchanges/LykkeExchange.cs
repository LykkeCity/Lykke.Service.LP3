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
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    public class LykkeExchange : ILykkeExchange, IStartable
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IOrderIdsMappingRepository _orderIdsMappingRepository;
        private readonly string _walletId;
        private readonly ILog _log;

        private readonly Dictionary<string, Dictionary<Guid, string>> _idsMap = 
            new Dictionary<string, Dictionary<Guid, string>>();
        
        public LykkeExchange(ILogFactory logFactory,
            IMatchingEngineClient matchingEngineClient,
            IOrderIdsMappingRepository orderIdsMappingRepository, 
            string walletId)
        {
            _matchingEngineClient = matchingEngineClient;
            
            _orderIdsMappingRepository = orderIdsMappingRepository;
            _walletId = walletId;

            _log = logFactory.CreateLog(this);
        }
        
        public async Task ApplyAsync(string assetPairId, IReadOnlyList<LimitOrder> limitOrders)
        {
            if (string.IsNullOrEmpty(_walletId))
                throw new Exception("WalletId is not set");

            var mapInternalToExternal = new Dictionary<Guid, string>();
            
            var multiOrderItems = new List<MultiOrderItemModel>();

            foreach (LimitOrder limitOrder in limitOrders)
            {
                var multiOrderItem = new MultiOrderItemModel
                {
                    Id = Guid.NewGuid().ToString("D"),
                    OrderAction = limitOrder.TradeType.ToOrderAction(),
                    Price = (double) limitOrder.Price,
                    Volume = (double) limitOrder.Volume,
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
                ClientId = _walletId,
                AssetPairId = assetPairId,
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

                // if order is not places, don't keep its it in mapping: it will be nothing to replace next time
                if (limitOrder.Error != LimitOrderError.None && limitOrder.Id != default)
                {
                    mapInternalToExternal.Remove(limitOrder.Id);
                }
            }

            await PersistMapping(assetPairId);
        }

        public async Task ResetIdsMappingAsync()
        {
            await _orderIdsMappingRepository.DeleteMappingsAsync();
            _idsMap.Clear();
            
            _log.Info("All ids mappings were deleted");
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
