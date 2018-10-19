using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    public class LykkeExchange : ILykkeExchange
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly string _walletId;
        private readonly ILog _log;

        public LykkeExchange(ILogFactory logFactory,
            IMatchingEngineClient matchingEngineClient,
            string walletId)
        {
            _matchingEngineClient = matchingEngineClient;
            _walletId = walletId;

            _log = logFactory.CreateLog(this);
        }
        
        public async Task ApplyAsync(string assetPairId, IReadOnlyCollection<LimitOrder> limitOrders)
        {
            if (string.IsNullOrEmpty(_walletId))
                throw new Exception("WalletId is not set");

            var map = new Dictionary<string, LimitOrder>();
            
            var multiOrderItems = new List<MultiOrderItemModel>();

            foreach (LimitOrder limitOrder in limitOrders)
            {
                var multiOrderItem = new MultiOrderItemModel
                {
                    Id = Guid.NewGuid().ToString("D"),
                    OrderAction = limitOrder.TradeType.ToOrderAction(),
                    Price = (double) limitOrder.Price,
                    Volume = (double) limitOrder.Volume,
                };

                map[multiOrderItem.Id] = limitOrder;
                
                multiOrderItems.Add(multiOrderItem);
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
                response = await _matchingEngineClient.PlaceMultiLimitOrderAsync(multiLimitOrder, 
                    new CancellationTokenSource(Consts.MatchingEngineTimeout).Token);
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
                if (map.TryGetValue(orderStatus.Id, out var limitOrder))
                {
                    limitOrder.Error = orderStatus.Status.ToOrderError();
                    limitOrder.ErrorMessage = limitOrder.Error != LimitOrderError.Unknown 
                        ? orderStatus.StatusReason
                        : !string.IsNullOrEmpty(orderStatus.StatusReason) ? orderStatus.StatusReason : "Unknown error";    
                }
            }
        }
    }
}
