using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.DtoModel;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.ClientSdk
{
    public class BlogClient : IBlogClient
    {
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<IBlogClient> _logger;

        private HttpClient? _httpClient;
		private string? _apiBaseUrl;

        public BlogClient(IHttpClientFactory httpClientFactory, ILogger<BlogClient> logger)
        {
			_httpClientFactory = httpClientFactory;
			_logger = logger;
        }

        public void Init(string apiBaseUrl)
        {
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                if (!apiBaseUrl.EndsWith("/api/blog"))
                    _apiBaseUrl = $"{apiBaseUrl}/api/blog";
            }
            else
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(apiBaseUrl));
            }

            _httpClient = _httpClientFactory.CreateClient();
        }


        public async Task<BlogEntitySet<TEntity>> GetEntitiesAsync<TEntity>(string accessToken, Guid? blogId = null, Guid? resourceId = null, int skip = 0, int count = 10, bool throwExceptions = true)
            where TEntity : DtoModelBase
        {
            try
            {
                if (_httpClient == null)
                    throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

                string requestUrl = blogId == null ? $"{_apiBaseUrl}" : $"{_apiBaseUrl}/{blogId.ToString()}/{typeof(TEntity).GetResourceName()}";

                if (blogId != null)
                    requestUrl = resourceId == null ? requestUrl : $"{requestUrl}/{resourceId.ToString()}";

                requestUrl = requestUrl.AddParameterToUri(nameof(skip), skip.ToString()).
                                        AddParameterToUri(nameof(count), count.ToString());
                
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(requestUrl)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return new BlogEntitySet<TEntity>(responseMessage.Content, responseMessage.StatusCode, throwExceptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity set of type '{EntityType}'", typeof(TEntity));

                if (!throwExceptions)
                {
                    return new BlogEntitySet<TEntity>(ex);
                }

                throw;
            }
        }

        public async Task<Uri?> CreateAsync<TEntity>(string accessToken, TEntity entityToCreate, bool throwExceptions = true) where TEntity : DtoModelBase
        {
            try
            {
				if (_httpClient == null)
					throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

				if (entityToCreate == null)
                    throw new ArgumentNullException(nameof(entityToCreate));

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

                string requestUrl = entityToCreate.BlogId == null ? $"{_apiBaseUrl}" : $"{_apiBaseUrl}/{entityToCreate.BlogId.ToString()}/{typeof(TEntity).GetResourceName()}";

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(requestUrl),
                    Content = new StringContent(JsonConvert.SerializeObject(entityToCreate), Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return responseMessage.StatusCode == HttpStatusCode.Created ? responseMessage.Headers.Location : null;
            }
            catch (Exception ex)
            {
                if (entityToCreate?.BlogId != null)
                {
                    _logger.LogError(ex, "Error creating {EntityType} for BlogId {BlogId})", typeof(TEntity), entityToCreate.BlogId.ToString());
                }
                else
                {
                    _logger.LogError(ex, "Error creating {EntityType}", typeof(TEntity));
                }

                if (!throwExceptions)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<bool> UpdateAsync<TEntity>(string accessToken, TEntity entityToUpdate, bool throwExceptions = true) where TEntity : DtoModelBase
        {
            try
            {
				if (_httpClient == null)
					throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

				if (entityToUpdate == null)
                    throw new ArgumentNullException(nameof(entityToUpdate));

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

                string requestUrl = entityToUpdate.ResourceId == null ? $"{_apiBaseUrl}/{entityToUpdate.BlogId.ToString()}" : $"{_apiBaseUrl}/{entityToUpdate.BlogId.ToString()}/{typeof(TEntity).GetResourceName()}/{entityToUpdate.ResourceId.GetValueOrDefault().ToString()}";

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(requestUrl),
                    Content = new StringContent(JsonConvert.SerializeObject(entityToUpdate), Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return responseMessage.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                if (entityToUpdate?.ResourceId != null)
                {
                    _logger.LogError(ex, "Error updating {EntityType} with Id {ResourceId} for BlogId {BlogId})", typeof(TEntity), entityToUpdate.ResourceId.GetValueOrDefault().ToString(), entityToUpdate.BlogId.GetValueOrDefault().ToString());
                }
                else
                {
                    _logger.LogError(ex, "Error updating {EntityType} with BlogId {BlogId}", typeof(TEntity), entityToUpdate?.BlogId.GetValueOrDefault().ToString());
                }

                if (!throwExceptions)
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<bool> DeleteAsync<TEntity>(string accessToken, Guid blogId, Guid? resourceId = null, bool throwExceptions = true) where TEntity : DtoModelBase
        {
            try
            {
				if (_httpClient == null)
					throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

				string requestUrl = resourceId == null ? $"{_apiBaseUrl}/{blogId.ToString()}" : $"{_apiBaseUrl}/{blogId.ToString()}/{typeof(TEntity).GetResourceName()}/{resourceId.GetValueOrDefault().ToString()}";

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(requestUrl),
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return responseMessage.StatusCode == HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                if (resourceId != null)
                {
                    _logger.LogError(ex, "Error deleting {EntityType} with Id {ResourceId} for BlogId {BlogId})", typeof(TEntity), resourceId.ToString(), blogId.ToString());
                }
                else
                {
                    _logger.LogError(ex, "Error deleting {EntityType} with BlogId {BlogId}", typeof(TEntity), blogId.ToString());
                }

                if (!throwExceptions)
                {
                    return false;
                }

                throw;
            }
        }

        public async Task<FileUploadResponse?> UploadFileAsync(string accessToken, byte[] fileBytes, string containerName, string fileName, bool overwrite = true, bool throwExceptions = true)
        {
            if (containerName == null)
                throw new ArgumentNullException(nameof(containerName));

            if (fileBytes == null)
                throw new ArgumentNullException(nameof(fileBytes));

            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (_httpClient == null)
                throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

            try
            {
                string requestUrl = $"{_apiBaseUrl}/blob";

                requestUrl = requestUrl.AddParameterToUri("overwrite", overwrite.ToString());

                var uploadRequest = new FileUploadRequest
                {
                    Base64Content = Convert.ToBase64String(fileBytes),
                    FileName = fileName,
                    ContainerName = containerName
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(requestUrl),
                    Content = new StringContent(JsonConvert.SerializeObject(uploadRequest), Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                if (!responseMessage.IsSuccessStatusCode)
                    return null;

                string? responseContent = await responseMessage.Content.ReadAsStringAsync();

                return string.IsNullOrWhiteSpace(responseContent) ? null : JsonConvert.DeserializeObject<FileUploadResponse>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file '{FileName}' to container {ContainerName}", fileName, containerName);

                if (!throwExceptions)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<byte[]?> GetFileAsync(string accessToken, string containerName, string fileName, bool throwExceptions = true)
        {
            if (containerName == null)
                throw new ArgumentNullException(nameof(containerName));

            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (_httpClient == null)
                throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

            try
            {
                string requestUrl = $"{_apiBaseUrl}/blob";

                requestUrl = requestUrl.AddParameterToUri(nameof(containerName), containerName).
                                        AddParameterToUri(nameof(fileName), fileName);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(requestUrl)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                if (responseMessage.IsSuccessStatusCode)
                {
                    return await responseMessage.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting  file '{FileName}' from container {ContainerName}", fileName, containerName);

                if (!throwExceptions)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string accessToken, string containerName, string fileName, bool throwExceptions = true)
        {
            if (containerName == null)
                throw new ArgumentNullException(nameof(containerName));

            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (_httpClient == null)
                throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

            try
            {
                string requestUrl = $"{_apiBaseUrl}/blob";

                requestUrl = requestUrl.AddParameterToUri(nameof(containerName), containerName).AddParameterToUri(nameof(fileName), fileName);


                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(requestUrl)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return responseMessage.IsSuccessStatusCode;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file '{FileName}' from container {ContainerName}", fileName, containerName);

                if (!throwExceptions)
                {
                    return false;
                }
                throw;
            }
        }
    }
}
