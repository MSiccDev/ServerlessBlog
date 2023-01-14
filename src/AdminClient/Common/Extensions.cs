using System.Text;
namespace MSiccDev.ServerlessBlog.AdminClient.Common
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

        public static string ToSeparatedValuesString(this string[] values, string separator = ",")
        {
            StringBuilder sb = new StringBuilder();

            foreach (string value in values)
                sb.Append($"{value}{separator}");

            return sb.ToString().Trim();
        }

    }
}
