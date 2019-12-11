using System;
using System.IO;
using System.Reflection;
using CommonLibrary.DataAccess;
using CommonLibrary.DataAccess.Abstractions;
using CommonLibrary.DataAccess.Manifest;
using CommonLibrary.ExceptionsMiddleware;
using Establishments.API.Events;
using Establishments.API.Models;
using Establishments.API.Services;
using Events;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Establishments.API
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
            services.AddMvc()
                .AddJsonOptions(opt => opt.SerializerSettings.Converters.Add(new StringEnumConverter()))
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<DatabaseConnectorSettings>(Configuration.GetSection(nameof(DatabaseConnectorSettings)));

            services.AddSingleton<IDatabaseConnectorSettings>(sp => { 
                return sp.GetRequiredService<IOptions<DatabaseConnectorSettings>>().Value;
            });

            services.AddSingleton<IRabbitConnector, RabbitConnector>();

            services.AddSingleton<IEstablishmentService, EstablishmentService>();

            services.AddSingleton<IControllerMessages, ControllerMessages>(cm => {
                return ManifestDataFormatter.ParseManifestDataToObject<ControllerMessages>("Establishments.API.Data.Messages.controllers.json").Result;
            });

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info { 
                    Title = "Establishments API", 
                    Version = "v1",
                    Description = "API de Estabelecimentos",
                    Contact = new Contact {
                        Name = "Luan M. Sales",
                        Email = "luanmsacc@gmail.com",
                        Url = "https://github.com/MulanSales"
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Establishments API v1");
                SubmitMethod[] methods = {
                    SubmitMethod.Get, 
                    SubmitMethod.Post, 
                    SubmitMethod.Options, 
                    SubmitMethod.Delete, 
                    SubmitMethod.Put
                };
                c.SupportedSubmitMethods(methods);
            });

            app.ConfigureExceptionHandler();
        }
    }
}
