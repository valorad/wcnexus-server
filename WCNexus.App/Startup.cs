using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using WCNexus.App.Database;
using WCNexus.App.Models;
using WCNexus.App.Services;

namespace WCNexus.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // add secrets
            services.Configure<DBConfig>(
                Configuration.GetSection("mongo")
            );

            services.AddSingleton<DBConfig>(sp =>
                sp.GetRequiredService<IOptions<DBConfig>>().Value
            );

            // configure db
            services.AddTransient<IDBContext, DBContext>();
            services.AddTransient<IDBCollection, DBCollection>();

            // provide services
            services.AddSingleton<INexusService, NexusService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IStoredProjectService, StoredProjectService>();
            services.AddSingleton<IPhotoService, PhotoService>();

            // others

            services.AddCors(options =>
            {
                options.AddPolicy("policy0", builder =>
                {
                    builder.AllowAnyHeader()
                            .WithMethods("GET", "POST", "PATCH", "DELETE")
                            .WithOrigins("*")
                            .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WCNexus.App", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WCNexus.App v1"));
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();

            app.UseCors("policy0");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
