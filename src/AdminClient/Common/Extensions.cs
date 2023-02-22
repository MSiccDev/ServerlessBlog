using System.Text;
namespace MSiccDev.ServerlessBlog.AdminClient.Common
{
    public static class Extensions
    {


        public static string ToSeparatedValuesString(this string[] values, string separator = ",")
        {
            StringBuilder sb = new StringBuilder();

            foreach (string value in values)
                sb.Append($"{value}{separator}");

            return sb.ToString().Trim();
        }

    }
}
