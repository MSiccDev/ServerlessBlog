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

        public event EventHandler? AuthorizationExpired;
        
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


        private async Task<List<TEntity>?> GetCachedBlogEntitiesAsync<TEntity>(Guid? blogId = null, string? resourceName = null, int cacheValidity = 30, bool forceRefresh = false, int count = 10, int skip = 0) where TEntity : DtoModelBase
        {
            string? key = null;
            resourceName = string.IsNullOrWhiteSpace(resourceName) ? string.Empty : $"_{resourceName}";
            key = !blogId.HasValue || blogId.GetValueOrDefault() == Guid.Empty ? "knownBlogs" : $"blog_{blogId}{resourceName}";

            List<TEntity>? result = Barrel.Current.Get<List<TEntity>>(key);

            if (!forceRefresh && !Barrel.Current.IsExpired(key))
                return result;

            var accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null)
            {
                _logger.LogError("Error fetching blogs: No valid AccessToken found in Storage");
                return result;
            }

            if (!string.IsNullOrWhiteSpace(accessTokenData.AccessToken))
            {
                BlogEntitySet<TEntity> apiResult = await _blogClient.GetEntitiesAsync<TEntity>(accessTokenData.AccessToken, blogId, null, skip, count).ConfigureAwait(false);

                if (apiResult.Value?.Any() ?? false)
                    result = apiResult.Value.Take(count).ToList();

                if (apiResult.Error != null)
                {
                    _logger.LogError("Error fetching blogs:\n {ErrorCode}:{ErrorMessage}", (int)apiResult.Error.StatusCode.GetValueOrDefault(), apiResult.Error.Message);

                    if (apiResult.Error.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        this.AuthorizationExpired?.Invoke(this, EventArgs.Empty);
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

        public async Task<Uri?> UploadFileAsync(byte[] fileBytes, string containerName, string fileName, bool overwriteExisting)
        {
            AzureAdAccessTokenResponse? accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null)
            {
                _logger.LogError("Error uploading file: No valid AccessToken found in Storage");
                return null;
            }

            if (string.IsNullOrWhiteSpace(accessTokenData.AccessToken))
                return null;

            FileUploadResponse? uploadResponse = await _blogClient.UploadFileAsync(accessTokenData.AccessToken, fileBytes, containerName, fileName).ConfigureAwait(false);

            return uploadResponse?.Url;

        }

        public async Task<byte[]?> GetFileAsync(Uri url)
        {
            byte[]? result = null;

            AzureAdAccessTokenResponse? accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null)
            {
                _logger.LogError("Error getting file: No valid AccessToken found in Storage");
                return result;
            }

            if (string.IsNullOrWhiteSpace(accessTokenData.AccessToken))
                return result;

            //https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes#language-support-for-indices-and-ranges
            string fileName = url.Segments[^1];

            if (string.IsNullOrWhiteSpace(fileName))
                return result;

            string containerName = url.Segments[^2];
            if (string.IsNullOrWhiteSpace(containerName))
                return result;

            result = await _blogClient.GetFileAsync(accessTokenData.AccessToken, containerName, fileName).ConfigureAwait(false);

            return result;
        }

        public async Task<bool> DeleteFileAsync(Uri url)
        {
            bool result = false;

            AzureAdAccessTokenResponse? accessTokenData = JsonConvert.DeserializeObject<AzureAdAccessTokenResponse>(await SecureStorage.Default.GetAsync(Constants.AzureAdAccessTokenStorageName));

            if (accessTokenData == null)
            {
                _logger.LogError("Error getting file: No valid AccessToken found in Storage");
                return result;
            }

            if (string.IsNullOrWhiteSpace(accessTokenData.AccessToken))
                return result;

            //https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes#language-support-for-indices-and-ranges
            string fileName = url.Segments[^1];

            if (string.IsNullOrWhiteSpace(fileName))
                return result;

            string containerName = url.Segments[^2];
            if (string.IsNullOrWhiteSpace(containerName))
                return result;

            result = await _blogClient.DeleteFileAsync(accessTokenData.AccessToken, containerName, fileName).ConfigureAwait(false);

            return result;
        }


        public async Task RefreshAsync(Guid blogId)
        {
            await GetAuthorsAsync(blogId, forceRefresh: true).ConfigureAwait(false);
            await GetTagsAsync(blogId, forceRefresh: true).ConfigureAwait(false);
            await GetMediaAsync(blogId, forceRefresh: true).ConfigureAwait(false);
            await GetPostsAsync(blogId, forceRefresh: true).ConfigureAwait(false);
        }

    }
}
