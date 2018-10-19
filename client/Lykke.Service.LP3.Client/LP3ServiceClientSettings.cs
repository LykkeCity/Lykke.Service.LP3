using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.LP3.Client 
{
    /// <summary>
    /// LP3 client settings.
    /// </summary>
    public class LP3ServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
