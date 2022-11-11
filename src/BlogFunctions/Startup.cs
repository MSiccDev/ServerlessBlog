using System;
using System.ComponentModel;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSiccDev.ServerlessBlog.EFCore;

//https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection

[assembly: FunctionsStartup(typeof(MSiccDev.ServerlessBlog.BlogFunctions.Startup))]

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            FunctionsHostBuilderContext functionContext = builder.GetContext();
            string connectionString = functionContext.Configuration["Database:ConnectionString"];

            builder.Services.AddDbContext<BlogContext>(options =>
                    SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));

        }
    }
}

