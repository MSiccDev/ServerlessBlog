using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MSiccDev.ServerlessBlog.EFCore.DesignDummy
{
    //https://docs.microsoft.com/en-gb/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
    public class BlogContextFactory : IDesignTimeDbContextFactory<BlogContext>
    {
        public BlogContext CreateDbContext(string[] args)
        {
            BlogContext? instance = null;

            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();

            optionsBuilder.UseSqlServer(dbContextBuilder =>
                dbContextBuilder.MigrationsAssembly("EFCore.DesignDummy"));

            instance = new BlogContext(optionsBuilder.Options);

            return instance;
        }
    }
}

