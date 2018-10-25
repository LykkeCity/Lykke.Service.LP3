using System.Net;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client;
using Lykke.Service.LP3.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.LP3.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("walletId")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_settingsService.GetWalletId());
        }
    }
}
