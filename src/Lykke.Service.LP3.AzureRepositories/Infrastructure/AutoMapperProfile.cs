using AutoMapper;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.CrossInstruments;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories.Infrastructure
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LevelEntity, Level>(MemberList.Destination)
                .ForMember(x => x.OriginalVolume, m => m.Ignore())
                .ConstructUsing(x => new Level(x.Name, x.Delta, x.Volume, x.VolumeBuy, x.VolumeSell, 
                    x.Inventory, x.OppositeInventory, x.Reference));
            
            CreateMap<Level, LevelEntity>(MemberList.Source)
                .ForSourceMember(x => x.Sell, m => m.Ignore())
                .ForSourceMember(x => x.Buy, m => m.Ignore())
                .ForMember(x => x.Volume, m => m.MapFrom(x => x.OriginalVolume));
            
            CreateMap<BaseAssetPairSettingsEntity, BaseAssetPairSettings>(MemberList.Destination);
            CreateMap<BaseAssetPairSettings, BaseAssetPairSettingsEntity>(MemberList.Source);

            CreateMap<AdditionalVolumeSettingsEntity, AdditionalVolumeSettings>(MemberList.Destination);
            CreateMap<AdditionalVolumeSettings, AdditionalVolumeSettingsEntity>(MemberList.Source);
            
            CreateMap<TradeEntity, Trade>(MemberList.Destination);
            CreateMap<Trade, TradeEntity>(MemberList.Source);
            
            CreateMap<CrossInstrumentEntity, CrossInstrument>(MemberList.Destination);
            CreateMap<CrossInstrument, CrossInstrumentEntity>(MemberList.Source);
        }
    }
}
