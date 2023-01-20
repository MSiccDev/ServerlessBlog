using System;
using System.Threading.Tasks;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.ClientSdk
{
    public interface IBlogClient
    {
        void Init(string apiBaseUrl);
		Task<BlogEntitySet<TEntity>> GetEntityList<TEntity>(string accessToken, Guid? blogId = null, int skip = 0, int count = 10, bool throwExceptions = true) where TEntity : DtoModelBase;
        Task<BlogEntity<TEntity>> GetEntityList<TEntity>(string accessToken, Guid blogId, Guid? resourceId = null, bool includeDetails = false, bool throwExceptions = true) where TEntity : DtoModelBase;
        Task<Uri?> Create<TEntity>(string accessToken, TEntity entityToCreate, bool throwExceptions = true) where TEntity : DtoModelBase;
        Task<bool> Update<TEntity>(string accessToken, TEntity entityToUpdate, bool throwExceptions = true) where TEntity : DtoModelBase;
        Task<bool> Delete<TEntity>(string accessToken, Guid blogId, Guid? resourceId = null, bool throwExceptions = true) where TEntity : DtoModelBase;
    }
}
