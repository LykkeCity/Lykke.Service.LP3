using AutoMapper;
using Lykke.Service.LP3.Client.Models.Settings;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LevelSettings, LevelSettingsModel>(MemberList.Source);
            CreateMap<LevelSettingsModel, LevelSettings>(MemberList.Destination);
        }
    }
}
