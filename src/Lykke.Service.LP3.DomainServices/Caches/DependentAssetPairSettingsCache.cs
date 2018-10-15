using System.Collections.Generic;
using System.Linq;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices.Caches
{
    public class DependentAssetPairSettingsCache
    {
        private List<AssetPairSettings> _assetPairSettings;

        public bool Initialized { get; private set; }

        public void Init(IEnumerable<AssetPairSettings> assetPairSettings)
        {
            _assetPairSettings = assetPairSettings.ToList();
            Initialized = true;
        }

        public IReadOnlyList<AssetPairSettings> GetAll()
        {
            return _assetPairSettings;
        }

        public void Clear()
        {
            Initialized = false;
        }
    }
}
