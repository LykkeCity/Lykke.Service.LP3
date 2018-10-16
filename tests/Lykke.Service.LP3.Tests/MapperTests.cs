using AutoMapper;
using Xunit;

namespace Lykke.Service.LP3.Tests
{
    public class MapperTests
    {
        [Fact]
        public void Test()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<AutoMapperProfile>());
            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void TestRepositoriesMapping()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<Lykke.Service.LP3.AzureRepositories.Infrastructure.AutoMapperProfile>());
            Mapper.AssertConfigurationIsValid();
        }
    }
}
