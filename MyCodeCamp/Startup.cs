using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using Newtonsoft.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using MyCodeCamp.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyCodeCamp.Controllers;
using MyCodeCamp.DTOs;

namespace MyCodeCamp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.environment = env;
            configuration = builder.Build();
        }

        IConfigurationRoot configuration;
        private IHostingEnvironment environment;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton(configuration);
            services.AddDbContext<CampContext>(ServiceLifetime.Scoped);
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();
            services.AddTransient<CampIdentityInitializer>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper();

            services.AddMemoryCache();

            services.AddIdentity<CampUser, IdentityRole>()
                .AddEntityFrameworkStores<CampContext>();

            services.Configure<IdentityOptions>(configure =>
            {
                configure.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = (context) =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = 401;
                        }
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = (context) =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = 403;
                        }
                        return Task.CompletedTask;
                    }
                };

            }); 

            services.AddApiVersioning(configure => 
            {
                // The  speakers controller uses Versioning Attributes
                configure.DefaultApiVersion = new ApiVersion(1, 1);
                configure.AssumeDefaultVersionWhenUnspecified = true;
                configure.ReportApiVersions = true;
                var reader = new QueryStringOrHeaderApiVersionReader("ver");
                reader.HeaderNames.Add("X-MyCodeCamp-Version");
                configure.ApiVersionReader = reader;


                // This is an alternative to Attribute versioning 
                configure.Conventions.Controller<TalksController>()
                .HasApiVersion(new ApiVersion(1, 0))
                .HasApiVersion(new ApiVersion(1, 1))
                .HasApiVersion(new ApiVersion(2, 0))
                .Action(a => a.Post(default(string), default(int), default(TalksDto)))
                    .MapToApiVersion(new ApiVersion(2, 0));
            });

            services.AddCors(configure =>
            {
                configure.AddPolicy("Wildermuth", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://wildermuth.com");
                });
                configure.AddPolicy("AnyGet", builder =>
                {
                    builder.AllowAnyHeader()
                    .WithMethods("GET")
                    .AllowAnyOrigin();
                });
            });

            services.AddAuthorization(configure =>
            {
                configure.AddPolicy("SuperUsers", p => p.RequireClaim("SuperUser", "True"));
            });


            // Add framework services.
            services.AddMvc(opt =>
            {
                if (!environment.IsProduction())
                {
                    opt.SslPort = 44388;
                }
                opt.Filters.Add(new RequireHttpsAttribute());
            })
              .AddJsonOptions(opt =>
              {
                  opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, CampDbInitializer seeder, CampIdentityInitializer identitySeeder)
        {
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIdentity();

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = this.configuration["Tokens:Issuer"],
                    ValidAudience = this.configuration["Tokens:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Tokens:Key"])),
                    ValidateLifetime = true
                }
            });

            app.UseMvc();

            seeder.Seed().Wait();
            identitySeeder.Seed().Wait();
        }
    }
}
