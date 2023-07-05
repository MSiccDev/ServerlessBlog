using System.Net;
using Azure;
using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MSiccDev.ServerlessBlog.DtoModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class BlobFunction
    {
        private const string Route = "blob";
        private readonly ILogger _logger;

        private JsonSerializerSettings? _jsonSerializerSettings;

        public BlobFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BlobFunction>();

            CreateNewtonSoftSerializerSettings();
        }

        private void CreateNewtonSoftSerializerSettings()
        {
            _jsonSerializerSettings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();

            _jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            _jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            _jsonSerializerSettings.Formatting = Formatting.Indented;
            _jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _jsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            _jsonSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
        }

        [OpenApiOperation("CREATE", "Blob", Description = "Creates a new blob for the attached file.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(FileUploadRequest), Required = true, Description = "The file to upload")]
        [OpenApiParameter("overwrite", In = ParameterLocation.Query, Type = typeof(bool), Required = false, Description = "overwrite existing files with the same name", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("ensureUnique", In = ParameterLocation.Query, Type = typeof(bool), Required = false, Description = "make sure the file name is unique", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "OK response if the file upload operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(BlobFunction)}_{nameof(Create)}")]
        public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                FileUploadRequest? fileUploadRequest = JsonConvert.DeserializeObject<FileUploadRequest>(requestBody);

                if (fileUploadRequest != null)
                {
                    byte[] fileBytes = Convert.FromBase64String(fileUploadRequest.Base64Content);

                    BlobServiceClient? blobServiceClient = GetBlobServiceClient();

                    if (blobServiceClient != null)
                    {
                        BlobContainerClient? containerClient = blobServiceClient.GetBlobContainerClient(fileUploadRequest.ContainerName);

                        _ = bool.TryParse(req.GetProperty("ensureUnique"), out bool ensureUnique);

                        string blobName = ensureUnique ? $"{Path.GetFileNameWithoutExtension(fileUploadRequest.FileName)}_{Guid.NewGuid()}{Path.GetExtension(fileUploadRequest.FileName)}" : fileUploadRequest.FileName;
                        
                        BlobClient? blobClient = containerClient.GetBlobClient(blobName);

                        _ = bool.TryParse(req.GetProperty("overwrite"), out bool overwrite);
                        
                        try
                        {
                            await blobClient.UploadAsync(new BinaryData(fileBytes), overwrite);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating blob container client");

                            if (ex is RequestFailedException requestFailedException)
                            {
                                if (requestFailedException.ErrorCode == "ContainerNotFound")
                                {
                                    await blobServiceClient.CreateBlobContainerAsync(fileUploadRequest.ContainerName);

                                    await blobClient.UploadAsync(new BinaryData(fileBytes), overwrite);
                                }
                            }
                            else
                            {
                                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
                            }
                        }

                        return await req.CreateResponseDataWithJsonAsync(HttpStatusCode.Created, new FileUploadResponse(blobClient.Uri), _jsonSerializerSettings);
                        
                    }

                    _logger.LogError("Error creating blob object because a BlobServiceClient couldn't be created");
                    return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
                }

                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted file upload is invalid, blob cannot be created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blob object");
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }

        [OpenApiOperation("GET", "Blob", Description = "Gets a file from the Azure Blob Storage.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("containerName", In = ParameterLocation.Query, Type = typeof(string), Required = true, Description = "container name to store the files in", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("fileName", In = ParameterLocation.Query, Type = typeof(string), Required = true, Description = "Name of the file to download", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/octet-stream", typeof(byte[]), Description = "Gets a file by its name in the Azure Blob Storage")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No file with the specified file name was found")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(BlobFunction)}_{nameof(GetFile)}")]
        public async Task<HttpResponseData> GetFile([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req)
        {
            string? fileName = req.GetProperty("fileName");
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogError("Error: file cannot be found without providing the file name");
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Please provide a valid file name.");
            }

            try
            {
                BlobServiceClient? blobServiceClient = GetBlobServiceClient();

                if (blobServiceClient != null)
                {
                    string? containerName = req.GetProperty("containerName");

                    BlobContainerClient? containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    BlobClient? blobClient = containerClient.GetBlobClient(fileName);

                    try
                    {
                        // ReSharper disable UseAwaitUsing
                        using Stream? stream = await blobClient.OpenReadAsync();
                        // ReSharper restore UseAwaitUsing

                        return await req.CreateBytesResponseAsync(HttpStatusCode.OK, stream, fileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting file blob with Name \'{Name}\'", fileName);

                        if (ex is RequestFailedException requestFailedEx)
                        {
                            if (requestFailedEx.ErrorCode == "BlobNotFound")
                                return await req.CreateResponseDataAsync(HttpStatusCode.NotFound, "The specified file does not exist on the server.");

                            return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, requestFailedEx.Message);
                        }
                        
                        return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
                    }
                }

                _logger.LogError("Error getting file because a BlobServiceClient couldn't be created");
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file blob with Name \'{Name}\'", fileName);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }





        [OpenApiOperation("DELETE", "Blob", Description = "Delete a blob from the Azure Storage.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("containerName", In = ParameterLocation.Query, Type = typeof(string), Required = true, Description = "container name to store the files in", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("fileName", In = ParameterLocation.Query, Type = typeof(string), Required = true, Description = "Name of the file to delete", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK response if the delete operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No file with the specified file name was found")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(BlobFunction)}_{nameof(Delete)}")]
        public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route)] HttpRequestData req)
        {
            string? fileName = req.GetProperty("fileName");
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogError("Error: file cannot be deleted without providing the file name");
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted file upload is invalid, blob cannot be created.");
            }

            try
            {
                BlobServiceClient? blobServiceClient = GetBlobServiceClient();

                if (blobServiceClient != null)
                {
                    string? containerName = req.GetProperty("containerName");

                    BlobContainerClient? containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    BlobClient? blobClient = containerClient.GetBlobClient(fileName);

                    try
                    {
                        await blobClient.DeleteIfExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting file blob with Name \'{Name}\'", fileName);
                        return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
                    }

                    return req.CreateResponse(HttpStatusCode.OK);
                }

                _logger.LogError("Error deleting file because a BlobServiceClient couldn't be created");
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file blob with Name \'{Name}\'", fileName);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }



        private BlobServiceClient? GetBlobServiceClient()
        {

            BlobServiceClient? blobServiceClient = null;
            string? storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
            string? blobServiceUrl = null;

#if DEBUG
            if (!string.IsNullOrWhiteSpace(storageAccountName))
                blobServiceUrl = $"https://127.0.0.1:10000/{storageAccountName}";
#else
                if (!string.IsNullOrWhiteSpace(storageAccountName))
                     blobServiceUrl = $"https://{storageAccountName}.blob.core.windows.net";
#endif

            if (!string.IsNullOrWhiteSpace(blobServiceUrl))
            {
                _logger.LogInformation("Using Blob Service Url: {Url}", blobServiceUrl);
                blobServiceClient = new BlobServiceClient(new Uri(blobServiceUrl), new DefaultAzureCredential());
            }
            else
            {
                _logger.LogError("Cannot read StorageAccountName setting for creation of blobServiceUrl");
            }

            return blobServiceClient;
        }
        
        
    }
}
