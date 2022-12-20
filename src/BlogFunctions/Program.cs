using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MSiccDev.ServerlessBlog.EFCore;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			string? sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

			IHost? host = new HostBuilder().
			              ConfigureFunctionsWorkerDefaults(worker => worker.UseNewtonsoftJson()).
			              ConfigureOpenApi().
			              ConfigureServices(services =>
			              {
				              if (!string.IsNullOrWhiteSpace(sqlConnectionString))
					              services.AddDbContext<BlogContext>(options =>
						              options.UseSqlServer(sqlConnectionString));

				              services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
				              {
					              OpenApiConfigurationOptions options = new OpenApiConfigurationOptions
					              {
						              Info = new OpenApiInfo
						              {
							              Version = "1.22501.0",
							              Title = "Serverless Blog API",
							              Description = "This is the API on which the serverless blog engine is running.",
							              TermsOfService = new Uri("https://github.com/MSiccDev/ServerlessBlog#readme"),
							              Contact = new OpenApiContact
							              {
								              Name = "MSiccDev Software Development",
								              Email = "info@msiccdev.net",
								              Url = new Uri("https://msiccdev.net")
							              },
							              License = new OpenApiLicense
							              {
								              Name = "License",
								              Url = new Uri("https://github.com/MSiccDev/ServerlessBlog/blob/main/LICENSE")
							              }
						              },
						              Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
						              OpenApiVersion = OpenApiVersionType.V3,
						              IncludeRequestingHostName = true,
						              ForceHttps = false,
						              ForceHttp = false
					              };
					              return options;
				              });
			              }).
			              Build();

			host.Run();
		}




	}
}
