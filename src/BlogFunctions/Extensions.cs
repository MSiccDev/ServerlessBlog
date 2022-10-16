using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public static class Extensions
    {
        public static (int count, int skip) GetPagingProperties(this HttpRequest req)
        {
            var queryParams = req.GetQueryParameterDictionary();

            int count = 10;
            int skip = 0;

            if (queryParams.Any(p => p.Key == nameof(count)))
                _ = int.TryParse(queryParams[nameof(count)], out count);

            if (queryParams.Any(p => p.Key == nameof(skip)))
                _ = int.TryParse(queryParams[nameof(skip)], out skip);

            return (count, skip);
        }
    }
}

