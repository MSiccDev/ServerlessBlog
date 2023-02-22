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
                if (!apiBaseUrl.EndsWith("/blog"))
                    _apiBaseUrl = $"{apiBaseUrl}/blog".Replace("//", "/");
            }
            else
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(apiBaseUrl));
            }

            _httpClient = _httpClientFactory.CreateClient();
        }


		public async Task<BlogEntitySet<TEntity>> GetEntityListAsync<TEntity>(string accessToken, Guid? blogId = null, int skip = 0, int count = 10, bool throwExceptions = true) where TEntity : DtoModelBase
        {
            try
            {
                if (_httpClient == null)
                    throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

                string requestUrl = blogId == null
                    ? $"{_apiBaseUrl}".
                      AddParameterToUri(nameof(skip), skip.ToString()).
                      AddParameterToUri(nameof(count), count.ToString())
                    : $"{_apiBaseUrl}/{blogId.ToString()}/{typeof(TEntity).GetResourceName()}".
                      AddParameterToUri(nameof(skip), skip.ToString()).
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
                _logger.LogError(ex, "Error getting list of {EntityType}", typeof(TEntity));

                if (!throwExceptions)
                {
                    return new BlogEntitySet<TEntity>(ex);
                }

                throw;
            }
        }

        public async Task<BlogEntity<TEntity>> GetEntityListAsync<TEntity>(string accessToken, Guid blogId, Guid? resourceId = null, bool includeDetails = false, bool throwExceptions = true)
            where TEntity : DtoModelBase
        {
            try
            {
				if (_httpClient == null)
					throw new ArgumentException("Please call the Init method first.", nameof(_httpClient));

				if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));

                string requestUrl = resourceId == null
                    ? $"{_apiBaseUrl}/{blogId.ToString()}/{typeof(TEntity).GetResourceName()}".
                        AddParameterToUri(nameof(includeDetails), includeDetails.ToString())
                    : $"{_apiBaseUrl}/{blogId.ToString()}/{typeof(TEntity).GetResourceName()}/{resourceId.ToString()}".
                        AddParameterToUri(nameof(includeDetails), includeDetails.ToString());

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(requestUrl)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return new BlogEntity<TEntity>(responseMessage.Content, responseMessage.StatusCode, throwExceptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType}", typeof(TEntity));

                if (!throwExceptions)
                {
                    return new BlogEntity<TEntity>(ex);
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

    }
}
