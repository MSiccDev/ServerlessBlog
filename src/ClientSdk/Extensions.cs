using System;
using System.Collections.Generic;
using MSiccDev.ServerlessBlog.DtoModel;
namespace MSiccDev.ServerlessBlog.ClientSdk
{
    public static class Extensions
    {
        public static string AddParameterToUri(this string url, string parameterName, string parameterValue)
        {
            if (url.Contains("?"))
            {
                return $"{url}&{parameterName}={parameterValue}";
            }
            return $"{url}?{parameterName}={parameterValue}";
        }

        public static string AddParametersToUri(this string url, Dictionary<string, string> parameters)
        {
            string result = url;

            if (parameters.Count > 0)
            {
                foreach (KeyValuePair<string, string> p in parameters)
                {
                    result = result.AddParameterToUri(p.Key, p.Value);
                }
            }

            return result;
        }

        public static string? GetResourceName(this Type type)
        {
            if (type == typeof(Blog))
                return nameof(Blog).ToLowerInvariant();

            if (type == typeof(Author))
                return nameof(Author).ToLowerInvariant();

            if (type == typeof(Medium))
                return "media";

            if (type == typeof(MediumType))
                return nameof(MediumType).ToLowerInvariant();

            if (type == typeof(Post))
                return nameof(Post).ToLowerInvariant();

            if (type == typeof(Tag))
                return nameof(Tag).ToLowerInvariant();


            throw new NotSupportedException($"Type {type} is not supported");
        }
    }
}
