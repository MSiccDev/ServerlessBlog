﻿using System;
using System.Linq;
using System.Net;
using Azure;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public static class Extensions
    {
        public static Dictionary<string, string> GetQueryParameterDictionary(this HttpRequestData req)
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


        public static async Task<HttpResponseData> CreateResponseDataAsync(this HttpRequestData req, HttpStatusCode statusCode, string? message)
        {
            HttpResponseData response = req.CreateResponse(statusCode);

            if (string.IsNullOrWhiteSpace(message))
                message = nameof(HttpStatusCode.BadRequest);

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

        public static async Task<HttpResponseData> CreateOKResponseDataWithJsonAsync(this HttpRequestData req, object responseData)
        {
            string json = JsonConvert.SerializeObject(responseData);

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            await response.WriteStringAsync(json);

            return response;
        }
    }
}

