using AutoMapper;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.States;

namespace Lykke.Service.LP3.AzureRepositories.Infrastructure
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LevelEntity, LevelSettings>(MemberList.Destination);
            CreateMap<LevelSettings, LevelEntity>(MemberList.Source);
            
            CreateMap<LevelEntity, LevelState>(MemberList.Destination);
            CreateMap<LevelState, LevelEntity>(MemberList.Source);
            
            CreateMap<BaseAssetPairSettingsEntity, BaseAssetPairSettings>(MemberList.Destination);
            CreateMap<BaseAssetPairSettings, BaseAssetPairSettingsEntity>(MemberList.Source);
        }
    }
}
