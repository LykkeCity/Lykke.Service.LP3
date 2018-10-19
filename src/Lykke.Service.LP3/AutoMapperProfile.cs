using AutoMapper;
using Lykke.Service.LP3.Client.Models;
using Lykke.Service.LP3.Client.Models.Balances;
using Lykke.Service.LP3.Client.Models.Orders;
using Lykke.Service.LP3.Client.Models.Settings;
using Lykke.Service.LP3.Client.Models.Trades;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<OrderBookTrader, OrderBookTraderSettingsModel>(MemberList.Destination);
            CreateMap<OrderBookTraderSettingsModel, OrderBookTraderSettings>(MemberList.Source);
            
            CreateMap<OrderBookTrader, OrderBookTraderModel>(MemberList.Source);
            
            CreateMap<LimitOrder, LimitOrderModel>(MemberList.Source);
            
            CreateMap<Trade, TradeModel>(MemberList.Source);

            CreateMap<Balance, AssetBalanceModel>(MemberList.Destination);
        }
    }
}
