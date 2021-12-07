using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;

namespace WebApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //string mongoDBConnectionString = _configuration.GetValue<string>("UserDatabaseSettings:ConnectionString");
            //MongoClient mongoClient = new MongoClient(mongoDBConnectionString);

            //if (mongoClient.Cluster.Description.State == MongoDB.Driver.Core.Clusters.ClusterState.Disconnected)
            //{
            //    throw new Exception("Could not connect to MongoDB using connection string '" + mongoDBConnectionString + "'." +
            //                        "\n If you are using a on-premise MongoDB, make sure your Mongod server is running." +
            //                        "\n If you are using  MongoDB Atlas, make sure your connection string is correct, including username and password.");
            //}

            services.AddSingleton<IMongoClient, MongoClient>(s =>
            {
                string mongoDBConnectionString = s.GetRequiredService<IConfiguration>()["UserDatabaseSettings:ConnectionString"];
                return new MongoClient(mongoDBConnectionString);
            });

            services.Configure<UserDatabaseSettings>(
                _configuration.GetSection(nameof(UserDatabaseSettings)));

            services.AddSingleton<IUserDatabseSettings>(sp =>
                sp.GetRequiredService<IOptions<UserDatabaseSettings>>().Value);

            // configure strongly typed settings objects
            var appSettingsSection = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            //reference to user service
            services.AddSingleton<UserService>();
            services.AddSingleton<EventsService>();
            services.AddSingleton<EventAttendanceService>();
            services.AddSingleton<PetitionsSevice>();
            services.AddSingleton<PetitionsSignedService>();
            services.AddSingleton<ThreadsService>();
            services.AddSingleton<ThreadMessagesService>();
            services.AddSingleton<APIStatsService>();

            //configure mapping
            services.AddAutoMapper(typeof(SereneMarineMappings));

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEventService, EventsService>();
            services.AddScoped<IEventAttendanceService, EventAttendanceService>();
            services.AddScoped<IPetitionService, PetitionsSevice>();
            services.AddScoped<IPetitionsSignedService, PetitionsSignedService>();
            services.AddScoped<IThreadsService, ThreadsService>();
            services.AddScoped<IThreadMessagesService, ThreadMessagesService>();
            services.AddScoped<IAPIStatsService, APIStatsService>();

            services.AddCors();
            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(swaggerGenOptions =>
            {
                //for token authorization
                swaggerGenOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n"
                                  + "Enter 'Bearer' [space] and then your token in the text input below."
                                  + "\r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                //tell swashbuckle which actions require authorization
                swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "SereneMarine API",
                    Description = "middleware for website and app",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Jacques",
                        Email = "jacqueso.olivier@gmail.com",
                        Url = new Uri("https://example.com"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swaggerGenOptions.IncludeXmlComments(xmlPath);
            });

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = context.Principal.Identity.Name.ToString();
                        var user = userService.GetById(userId);
                        if (user == null)
                        {
                            // return unauthorized if user no longer exists
                            context.Fail("Unauthorized");
                        }
                        return Task.CompletedTask;
                    }
                };
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(swaggerUIOptions =>
            {
                swaggerUIOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //serve swagger ui at top root
                swaggerUIOptions.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            // global cors policy
            app.UseCors(corsPolicyBuiler => corsPolicyBuiler
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}