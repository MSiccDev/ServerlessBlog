using System.Net;
using Microsoft.Extensions.Logging;
using MonkeyCache;
using MonkeyCache.SQLite;
using MSiccDev.ServerlessBlog.AdminClient.Common;
using MSiccDev.ServerlessBlog.ClientSdk;
using MSiccDev.ServerlessBlog.DtoModel;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public class CacheService : ICacheService
    {
        private readonly IBlogClient _blogClient;
        private readonly ILogger<ICacheService> _logger;
        private bool _debugLocally;

        public event EventHandler? AuthorizationExpired;
        public event EventHandler<RequestError?>? ApiErrorOccured;

        public event EventHandler? CacheHasChanged;
        
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

            _debugLocally = Preferences.Get(Constants.DebugLocallyStorageName, false);
        }


        public async Task<List<BlogOverview>?> GetBlogsAsync(int cacheValidity = 30, bool forceRefresh = false)
            => await GetCachedBlogEntitiesAsync<BlogOverview>(null, null, cacheValidity, forceRefresh);

        public async Task<List<Author>?> GetAuthorsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 10, int skip = 0)
            => await GetCachedBlogEntitiesAsync<Author>(blogId, typeof(Author).GetResourceName(), cacheValidity, forceRefresh, count, skip);

        public async Task<List<Tag>?> GetTagsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 100, int skip = 0)
            => await GetCachedBlogEntitiesAsync<Tag>(blogId, typeof(Tag).GetResourceName(), cacheValidity, forceRefresh, count, skip);

        public async Task<List<Medium>?> GetMediaAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 100, int skip = 0)
            => await GetCachedBlogEntitiesAsync<Medium>(blogId, typeof(Medium).GetResourceName(), cacheValidity, forceRefresh, count, skip);

        public async Task<List<Post>?> GetPostsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 25, int skip = 0)
            => await GetCachedBlogEntitiesAsync<Post>(blogId, typeof(Post).GetResourceName(), cacheValidity, forceRefresh, count, skip);


        private async Task<List<TEntity>?> GetCachedBlogEntitiesAsync<TEntity>(Guid? blogId = null, string? resourceName = null, int cacheValidity = 30, bool forceRefresh = false, int count = 10, int skip = 0)
            where TEntity : DtoModelBase
        {
            string? key = null;
            resourceName = string.IsNullOrWhiteSpace(resourceName) ? string.Empty : $"_{resourceName}";
            key = !blogId.HasValue || blogId.GetValueOrDefault() == Guid.Empty ? "knownBlogs" : $"blog_{blogId}{resourceName}";

            List<TEntity>? result = Barrel.Current.Get<List<TEntity>>(key);

            if (!forceRefresh && !Barrel.Current.IsExpired(key))
                return result;

            AzureAdAccessTokenResponse? accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null && !_debugLocally)
            {
                _logger.LogError("Error fetching blogs: No valid AccessToken found in Storage");
                return result;
            }

            if (!string.IsNullOrWhiteSpace(accessTokenData.AccessToken) || _debugLocally)
            {
                BlogEntitySet<TEntity> apiResult = await _blogClient.GetEntitiesAsync<TEntity>(accessTokenData.AccessToken, blogId, null, skip, count, false);

                if (apiResult.Value?.Any() ?? false)
                    result = apiResult.Value.Take(count).ToList();

                if (apiResult.Error != null)
                {
                    _logger.LogError("Error fetching blogs:\n {ErrorCode}:{ErrorMessage}", (int)apiResult.Error.StatusCode.GetValueOrDefault(), apiResult.Error.Message);

                    if (apiResult.Error.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        this.AuthorizationExpired?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        this.ApiErrorOccured?.Invoke(this, apiResult.Error);
                    }
                }
                
                Barrel.Current.Empty(key);
                Barrel.Current.Add(key, result, TimeSpan.FromDays(cacheValidity));
            }

            return result;
        }

        public async Task<BlogOverview?> GetCurrentBlogAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false)
        {
            List<BlogOverview>? blogs = await GetBlogsAsync(cacheValidity, forceRefresh);
            BlogOverview? currentBlog = blogs?.SingleOrDefault(blog => blog.BlogId == blogId);

            if (forceRefresh && currentBlog != null)
                await RefreshAsync(currentBlog.BlogId!.Value);

            return currentBlog;
        }

        public async Task RefreshAsync(Guid blogId)
        {
            await GetAuthorsAsync(blogId, forceRefresh: true).ConfigureAwait(false);
            await GetTagsAsync(blogId, forceRefresh: true).ConfigureAwait(false);
            await GetMediaAsync(blogId, forceRefresh: true).ConfigureAwait(false);
            await GetPostsAsync(blogId, forceRefresh: true).ConfigureAwait(false);
        }

        public async Task DeleteAsync<TEntity>(Guid blogId, Guid? resourceId)
            where TEntity : DtoModelBase
        {
            AzureAdAccessTokenResponse? accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null && !_debugLocally)
            {
                _logger.LogError("Error deleting entity from blog with {BlogId} and {ResourceId}: No valid AccessToken found in Storage", blogId, resourceId);
                return;
            }

            if (!string.IsNullOrWhiteSpace(accessTokenData!.AccessToken) || _debugLocally)
            {
                bool deleted = await _blogClient.DeleteAsync<TEntity>(accessTokenData.AccessToken!, blogId, resourceId);

                if (deleted)
                    this.CacheHasChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task UpdateAsync<TEntity>(TEntity entityToUpdate)
            where TEntity : DtoModelBase
        {
            AzureAdAccessTokenResponse? accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null && !_debugLocally)
            {
                _logger.LogError("Error updating entity on blog with {BlogId} and {ResourceId}: No valid AccessToken found in Storage", entityToUpdate.BlogId.GetValueOrDefault(), entityToUpdate.ResourceId.GetValueOrDefault());
                return;
            }

            if (!string.IsNullOrWhiteSpace(accessTokenData!.AccessToken) || _debugLocally)
            {
                bool updated = await _blogClient.UpdateAsync(accessTokenData.AccessToken!, entityToUpdate);

                if (updated)
                    this.CacheHasChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
