using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices.Assets
{
    [UsedImplicitly]
    public class AssetLinkService : IAssetLinkService
    {
        private readonly InMemoryCache<AssetLink> _cache;
        private readonly ILog _log;

        public AssetLinkService(IReadOnlyCollection<AssetMapping> mappingSettings, ILogFactory logFactory)
        {
            _cache = new InMemoryCache<AssetLink>(assetLink => assetLink.AssetId, false);
            _log = logFactory.CreateLog(this);

            IReadOnlyCollection<AssetLink> assetLinks = mappingSettings.Select(o => new AssetLink
            {
                AssetId = o.AssetId,
                ExternalAssetId = o.ExternalAssetId
            }).ToArray();

            _cache.Initialize(assetLinks);
        }

        public Task<IReadOnlyCollection<AssetLink>> GetAllAsync()
        {
            IReadOnlyCollection<AssetLink> assetLinks = _cache.GetAll();

            return Task.FromResult(assetLinks);
        }
    }
}
