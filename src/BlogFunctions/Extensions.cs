﻿using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public static class Extensions
    {
        private static Dictionary<string, string> GetQueryParameterDictionary(this HttpRequestData req)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string queryParams = req.Url.GetComponents(UriComponents.Query, UriFormat.UriEscaped);

            if (!string.IsNullOrWhiteSpace(queryParams))
            {
                string[] paramSplits = queryParams.Split('&');

                if (paramSplits.Any())
                {
                    foreach (string split in paramSplits)
                    {
                        string[] valueSplits = split.Split('=');

                        if (valueSplits.Any() && valueSplits.Length == 2)
                            result.Add(valueSplits[0], valueSplits[1]);
                    }
                }
            }
            return result;
        }

        public static (int count, int skip) GetPagingProperties(this HttpRequestData req)
        {
            Dictionary<string, string> queryParams = req.GetQueryParameterDictionary();

            int count = 10;
            int skip = 0;

            if (queryParams.Any(p => p.Key == nameof(count)))
                _ = int.TryParse(queryParams[nameof(count)], out count);

            if (queryParams.Any(p => p.Key == nameof(skip)))
                _ = int.TryParse(queryParams[nameof(skip)], out skip);

            return (count, skip);
        }

        public static string? GetProperty(this HttpRequestData req, string name)
        {
            string result = null;
            Dictionary<string, string> queryParams = req.GetQueryParameterDictionary();

            if (queryParams.Any(p => p.Key == name))
                result = queryParams[name];

            return result;
        }

        public static async Task<HttpResponseData> CreateResponseDataAsync(this HttpRequestData req, HttpStatusCode statusCode, string? message, bool isJson = false)
        {
            HttpResponseData response = req.CreateResponse(statusCode);

            if (string.IsNullOrWhiteSpace(message))
                message = statusCode.ToString();

            response.Headers.Add("Content-Type", isJson ? "application/json; charset=utf-8" : "application/text; charset=utf-8");

            await response.WriteStringAsync(message);

            return response;
        }

        public static async Task<HttpResponseData> CreateNewEntityCreatedResponseDataAsync(this HttpRequestData req, Guid createdResourceId)
        {
            HttpResponseData response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Location", $"{req.Url}/{createdResourceId}");

            await response.WriteStringAsync("OK");

            return response;
        }

        public static async Task<HttpResponseData> CreateOkResponseDataWithJsonAsync(this HttpRequestData req, object responseData, JsonSerializerSettings? settings)
        {
            string json = JsonConvert.SerializeObject(responseData, settings);
            
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(json);

            return response;
        }

        public static async Task<HttpResponseData> CreateResponseDataWithJsonAsync(this HttpRequestData req, HttpStatusCode statusCode, object responseData, JsonSerializerSettings? settings)
        {
            string json = JsonConvert.SerializeObject(responseData, settings);

            HttpResponseData response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(json);

            return response;
        }

        public static async Task<HttpResponseData> CreateBytesResponseAsync(this HttpRequestData req, HttpStatusCode statusCode, Stream stream, string fileName)
        {
            HttpResponseData response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/octet-stream");
            response.Headers.Add("Content-disposition", $"attachment; filename=\"{fileName}\"");

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            await response.WriteBytesAsync(memoryStream.ToArray());

            return response;
        }
    }
}

