using System;
using System.Text.RegularExpressions;
using Aloha.Controllers;
using Aloha.Dtos;
using Aloha.Model.Entities;
using Aloha.Model.Repositories;
using Aloha.Models.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Aloha.Mappers;

namespace Aloha
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddCors();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(swagger =>
            {
                var info = new Info()
                {
                    Title = SwaggerConfig.DocInfoTitle,
                    Version = SwaggerConfig.DocInfoVersion,
                    Description = SwaggerConfig.DocInfoDescription,
                    Contact = new Contact() { Name = SwaggerConfig.ContactName, Url = SwaggerConfig.ContactUrl }
                };

                swagger.SwaggerDoc(SwaggerConfig.DocNameV1, info);
            });

            services.AddDbContext<AlohaContext>(options =>
            {
                string connectionString = Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
                string portNumber = Regex.Match(connectionString, @"(?<=Data Source.+:)\d+")?.Value;

                if (portNumber != null && portNumber.Length > 0)
                {
                    connectionString += ";Port=" + portNumber;
                    connectionString = connectionString.Replace(":" + portNumber, string.Empty);
                }

                options.UseMySql(connectionString);
            });

            // Repositories
            services.AddScoped<IRepository<Worker>, Repository<Worker>>();

            // Controllers
            services.AddScoped<WorkerController, WorkerController>();
            services.AddScoped<FloorController, FloorController>();
            services.AddScoped<OfficeController, OfficeController>();

            // Mappings
            services.AddScoped<IClassMapping<Worker, WorkerDto>, WorkerToWorkerDtoMapping>();
            services.AddScoped<IClassMapping<WorkerDto, Worker>, WorkerDtoToWorkerMapping>();
            services.AddScoped<IClassMapping<Floor, FloorDto>, FloorToFloorDtoMapping>();
            services.AddScoped<IClassMapping<FloorDto, Floor>, FloorDtoToFloorMapping>();
            services.AddScoped<IClassMapping<Office, OfficeDto>, OfficeToOfficeDtoMapping>();
            services.AddScoped<IClassMapping<OfficeDto, Office>, OfficeDtoToOfficeMapping>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint(SwaggerConfig.EndpointUrl, SwaggerConfig.EndpointDescription));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(builder => 
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<AlohaContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
