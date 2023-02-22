using Microsoft.Extensions.Logging;
using MonkeyCache;
using MonkeyCache.SQLite;
using MSiccDev.ServerlessBlog.ClientSdk;
namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public class CacheService : ICacheService
    {
        private readonly IBlogClient _blogClient;
        private readonly ILogger<ICacheService> _logger;

        public CacheService(IBlogClient blogClient, ILogger<CacheService> logger)
        {
            _blogClient = blogClient;
            _logger = logger;
        }

        public void Init(string id, string? path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                BarrelUtils.SetBaseCachePath(path);

            Barrel.ApplicationId = id;
        }
    }
}
