using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IOrderIdsMappingRepository
    {
        Task PersistMapping(string assetPairId, Dictionary<Guid, string> mapping);

        Task<Dictionary<string, Dictionary<Guid, string>>> RestoreMapping();
    }
}
