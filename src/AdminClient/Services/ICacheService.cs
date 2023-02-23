using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.AdminClient.Services
{
    public interface ICacheService
    {
        void Init(string id, string? path = null);

        Task<List<BlogOverview>?> GetBlogsAsync(int cacheValidity = 30, bool forceRefresh = false);
        Task<List<Author>?> GetAuthorsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false);
        Task<List<Tag>?> GetTagsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false);
        Task<List<Medium>?> GetMediaAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false);
        Task<List<Post>?> GetPostsAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false);

        Task<BlogOverview?> GetCurrentBlogAsync(Guid blogId, int cacheValidity = 30, bool forceRefresh = false);
    }
}
