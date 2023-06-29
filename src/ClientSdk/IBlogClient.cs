using System;
using System.Threading.Tasks;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.ClientSdk
{
    public interface IBlogClient
    {
        void Init(string apiBaseUrl);
        Task<BlogEntitySet<TEntity>> GetEntitiesAsync<TEntity>(string accessToken, Guid? blogId = null, Guid? resourceId = null, int skip = 0, int count = 10, bool throwExceptions = true) where TEntity : DtoModelBase;        
        Task<Uri?> CreateAsync<TEntity>(string accessToken, TEntity entityToCreate, bool throwExceptions = true) where TEntity : DtoModelBase;
        Task<bool> UpdateAsync<TEntity>(string accessToken, TEntity entityToUpdate, bool throwExceptions = true) where TEntity : DtoModelBase;
        Task<bool> DeleteAsync<TEntity>(string accessToken, Guid blogId, Guid? resourceId = null, bool throwExceptions = true) where TEntity : DtoModelBase;

        Task<FileUploadResponse?> UploadFileAsync(string accessToken, byte[] fileBytes, string containerName, string fileName, bool overwrite = true, bool ensureUnique = false, bool throwExceptions = true);

        Task<byte[]?> GetFileAsync(string accessToken, string containerName, string fileName, bool throwExceptions = true);

        Task<bool> DeleteFileAsync(string accessToken, string containerName, string fileName, bool throwExceptions = true);

    }
}
