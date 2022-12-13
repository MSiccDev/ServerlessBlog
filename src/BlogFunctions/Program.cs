using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSiccDev.ServerlessBlog.EFCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string? sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            IHost? host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults((IFunctionsWorkerApplicationBuilder workerApplication) =>
                {
                    workerApplication.Services.Configure<WorkerOptions>(workerOptions =>
                    {
                        JsonSerializerSettings jsonSerializerSettings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();

                        jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                        jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        jsonSerializerSettings.Formatting = Formatting.Indented;
                        jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        jsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        jsonSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;

                        workerOptions.Serializer = new NewtonsoftJsonObjectSerializer(jsonSerializerSettings);
                    });
                })
                .ConfigureServices(services =>
                {
                    if (!string.IsNullOrWhiteSpace(sqlConnectionString))
                        services.AddDbContext<BlogContext>(options =>
                            options.UseSqlServer(sqlConnectionString));
                })
                .Build();

            host.Run();
        }
    }
}
