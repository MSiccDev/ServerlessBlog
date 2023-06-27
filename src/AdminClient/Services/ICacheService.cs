using MSiccDev.ServerlessBlog.ClientSdk;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public interface ICacheService
    {
        void Init(string id, string? path = null);


        event EventHandler? AuthorizationExpired;
        event EventHandler<RequestError?>? ApiErrorOccured;

        public event EventHandler? CacheHasChanged;
        
        Task<List<BlogOverview>?> GetBlogsAsync(int cacheValidity = 30, bool forceRefresh = false);
        Task<List<Author>?> GetAuthorsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 10, int skip = 0);
        Task<List<Tag>?> GetTagsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 100, int skip = 0);
        Task<List<Medium>?> GetMediaAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 10, int skip = 0);
        Task<List<Post>?> GetPostsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false, int count = 25, int skip = 0);

        Task<BlogOverview?> GetCurrentBlogAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false);

        Task<Uri?> UploadFileAsync(byte[] fileBytes, string containerName, string fileName, bool overwriteExisting);

        
        Task RefreshAsync(Guid blogId);


        Task DeleteAsync<TEntity>(Guid blogId, Guid? resourceId) where TEntity : DtoModelBase;

        Task UpdateAsync<TEntity>(TEntity entityToUpdate) where TEntity : DtoModelBase;
    }
}
