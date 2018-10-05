using AutoMapper;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories.Infrastructure
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LevelSettingsEntity, LevelSettings>(MemberList.Destination);
            CreateMap<LevelSettings, LevelSettingsEntity>(MemberList.Source);
        }
    }
}
