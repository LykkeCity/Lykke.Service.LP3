using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class LimitOrderStore : ILimitOrderStore
    {
        private string _assetPairId;
        private List<LimitOrder> _orders;
        private readonly ILimitOrderService _limitOrderService;
        
        public Task MarkOrdersDisabled(bool isDisabled)
        {
            if (isDisabled)
            {
                _orders.ForEach(x =>
                {
                    x.Error = LimitOrderError.OrderBookIsDisabled;
                    x.ErrorMessage = "Order book is disabled";
                });
            }
            else
            {
                _orders.Where(x => x.Error == LimitOrderError.OrderBookIsDisabled).ToList().ForEach(x =>
                {
                    x.Error = LimitOrderError.None;
                    x.ErrorMessage = null;
                });
            }

            return Task.CompletedTask;
        }

        public Task ClearAndAddOrders(IEnumerable<LimitOrder> orders)
        {
            _orders = orders.ToList();

            return Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<LimitOrder>> GetOrders(TradeType? tradeType = default)
        {
            if (tradeType.HasValue)
                return await Task.FromResult(_orders.Where(x => x.TradeType == tradeType.Value).ToList().AsReadOnly());
            else
                return await Task.FromResult(_orders.AsReadOnly());
        }

        public Task AddSingleOrder(LimitOrder order)
        {
            _orders.Add(order);

            return Task.CompletedTask;
        }

        public async Task<LimitOrder> RemoveSingleOrder(Guid id)
        {
            var order = _orders.FirstOrDefault(x => x.Id == id);
            
            if(order == null)
                throw new ArgumentException(nameof(id));
            
            _orders.Remove(order);

            return order;
        }

        public Task Clear()
        {
            _orders.Clear();

            return Task.CompletedTask;
        }

        public Task PersistOrder(LimitOrder order)
        {
            if(_orders.All(x => x.Id != order.Id))
                throw new ArgumentException(nameof(order));
            
            return Task.CompletedTask;
        }
    }
}
