using AutoMapper;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3.AzureRepositories.Infrastructure
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<OrderBookTrader, OrderBookEntity>(MemberList.Source);
            CreateMap<OrderBookEntity, OrderBookTrader>(MemberList.Destination);
            
            CreateMap<TradeEntity, Trade>(MemberList.Destination);
            CreateMap<Trade, TradeEntity>(MemberList.Source);
        }
    }
}
