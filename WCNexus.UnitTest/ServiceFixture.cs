using WCNexus.App.Database;
using WCNexus.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WCNexus.App.Models;

namespace WCNexus.UnitTest
{
    public class ServiceFixture
    {
        public ServiceProvider ServiceProvider { get; }

        public ServiceFixture()
        {
            var config = new ConfigurationBuilder()
                .AddYamlFile("secrets.yaml")
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            // add secrets
            services.Configure<DBConfig>(
              config.GetSection("mongo")
            );

            services.AddSingleton<DBConfig>(sp =>
              sp.GetRequiredService<IOptions<DBConfig>>().Value
            );

            // config db
            services.AddTransient<IDBContext, DBContext>();
            services.AddTransient<IDBCollection, DBCollection>();

            // add services
            services.AddSingleton<INexusService, NexusService>();
            services.AddSingleton<IStoredProjectService, StoredProjectService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IPhotoService, PhotoService>();

            ServiceProvider = services.BuildServiceProvider();
        }

    }
}