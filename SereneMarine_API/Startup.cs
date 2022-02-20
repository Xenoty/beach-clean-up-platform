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
using MongoDB.Driver.Core.Clusters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;

namespace WebApi
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private IMongoClient client;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // When the application is started, check whether a connection can be established to MongoDB
            //var testing = client.Settings.ConnectTimeout.TotalMilliseconds; // Default timeout for mongoDb
            int maxTimeToWaitInMilliSecondsForTimeout = _configuration.GetValue<int>("UserDatabaseSettings:Timeout");
            int intervalTimeToWaitInMilliSeconds = _configuration.GetValue<int>("UserDatabaseSettings:Interval");
            int currentTimeWaitedForInMilliSeconds = 0;

            string mongoDBConnectionString = _configuration.GetValue<string>("UserDatabaseSettings:ConnectionString");
            client = new MongoClient(mongoDBConnectionString);

            while (client.Cluster.Description.State != ClusterState.Connected)
            {
                if (currentTimeWaitedForInMilliSeconds >= maxTimeToWaitInMilliSecondsForTimeout)
                {
                    throw new Exception("Could not connect to MongoDB using connection string '" + mongoDBConnectionString + "'." +
                    "\n If you are using a on-premise MongoDB, make sure your Mongod server is running." +
                    "\n If you are using  MongoDB Atlas, make sure your connection string is correct, including username and password.");
                }

                // Sleep for 0.1 seconds then try again
                System.Threading.Thread.Sleep(intervalTimeToWaitInMilliSeconds);
                currentTimeWaitedForInMilliSeconds += intervalTimeToWaitInMilliSeconds;
            }

            bool loadDefaultData = _configuration.GetValue<bool>("AppSettings:LoadDefaultData");
            if (loadDefaultData)
            {
                // Load default data
                UserDatabaseSettings userDatabaseSettings = CreateUserDatabaseSettings();
                // Create Services
                UserService userService = new UserService(client, userDatabaseSettings);
                EventsService eventsService = new EventsService(client, userDatabaseSettings, _configuration);
                PetitionsSevice petitionsSevice = new PetitionsSevice(client, userDatabaseSettings);
                ThreadsService threadsService = new ThreadsService(client, userDatabaseSettings);
                // Create dummy data
                User user = CreateAdminUser(userService);
                CreateUpcommingEvents(user, eventsService);
                CreatePastEvents(user, eventsService);
                CreatePetitions(user, petitionsSevice);
                CreateThreads(user, threadsService);
                
                // Set load default data to false
                UpdateAppSetting("AppSettings:LoadDefaultData", "false");
            }

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

        private UserDatabaseSettings CreateUserDatabaseSettings()
        {
            return new UserDatabaseSettings()
            {
                UsersCollectionName = _configuration["UserDatabaseSettings:UsersCollectionName"],
                EventsCollectionName = _configuration["UserDatabaseSettings:EventsCollectionName"],
                EventAttendanceCollectionName = _configuration["UserDatabaseSettings:EventAttendanceCollectionName"],
                PetitionsCollectionName = _configuration["UserDatabaseSettings:PetitionsCollectionName"],
                PetitionsSignedCollectionName = _configuration["UserDatabaseSettings:PetitionsSignedCollectionName"],
                ThreadsCollectionName = _configuration["UserDatabaseSettings:ThreadsCollectionName"],
                ThreadMessagesCollectionName = _configuration["UserDatabaseSettings:ThreadMessagesCollectionName"],
                DatabaseName = _configuration["UserDatabaseSettings:DatabaseName"]
            };
        }

        private User CreateAdminUser(UserService userService)
        {
            // Create the admin user
            User user = new User()
            {
                FirstName = "Admin",
                LastName = "User",
                Email_address = "admin@serenemarine.com",
                Role = "Admin",
                Joined = DateTime.Now,
                User_Id = Guid.NewGuid().ToString(),
            };

            try
            {
                userService.Create(user, "123456");
            }
            catch (AppException appException)
            {
                Console.WriteLine(appException.Message);
            }

            return user;
        }

        private void CreateUpcommingEvents(User user, EventsService eventsService)
        {
            // Create 3 Upcomming Events, 4 past events

            Event ev1 = new Event()
            {
                event_id = Guid.NewGuid().ToString(),
                event_name = "Sample Event 1",
                event_descr = "Sample Event 1 description. Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                User_Id = user.User_Id,
                longitude = -29.852241f,
                latitude = 31.039504f,
                address = "North Beach, Durban, KwaZulu-Natal",
                event_startdate = DateTime.Now.AddDays(1),
                event_enddate = DateTime.Now.AddDays(1).AddHours(4),
                event_createddate = DateTime.Now,
                max_attendance = 75,
                event_completed = false
            };

            Event ev2 = new Event()
            {
                event_id = Guid.NewGuid().ToString(),
                event_name = "Sample Event 2",
                event_descr = "Sample Event 2 description. Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                User_Id = user.User_Id,
                longitude = -29.862501f,
                latitude = 31.04359f,
                address = "South Beach, Durban, KwaZulu-Natal",
                event_startdate = DateTime.Now.AddDays(2),
                event_enddate = DateTime.Now.AddDays(2).AddHours(4),
                event_createddate = DateTime.Now,
                max_attendance = 50,
                event_completed = false
            };

            Event ev3 = new Event()
            {
                event_id = Guid.NewGuid().ToString(),
                event_name = "Sample Event 3",
                event_descr = "Sample Event 3 description. Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                User_Id = user.User_Id,
                longitude = -29.829147f,
                latitude = 31.037263f,
                address = "Bike & Bean, Durban, KwaZulu-Natal",
                event_startdate = DateTime.Now.AddDays(3),
                event_enddate = DateTime.Now.AddDays(3).AddHours(4),
                event_createddate = DateTime.Now,
                max_attendance = 100,
                event_completed = false
            };

            try
            {
                eventsService.Create(ev1);
                eventsService.Create(ev2);
                eventsService.Create(ev3);
            }
            catch (AppException appException)
            {

                Console.WriteLine(appException.Message);
            }
        }

        private void CreatePastEvents(User user, EventsService eventsService)
        {
            Event pastEvent1 = new Event()
            {
                event_id = Guid.NewGuid().ToString(),
                event_name = "Past Event 1",
                event_descr = "Past Event 1 description. Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                User_Id = user.User_Id,
                longitude = -29.852241f,
                latitude = 31.039504f,
                address = "North Beach, Durban, KwaZulu-Natal",
                event_startdate = DateTime.Now.AddDays(-1),
                event_enddate = DateTime.Now.AddDays(-1).AddHours(4),
                event_createddate = DateTime.Now,
                max_attendance = 50,
                event_completed = true
            };

            Event pastEvent2 = new Event()
            {
                event_id = Guid.NewGuid().ToString(),
                event_name = "Past Event 2",
                event_descr = "Past Event 2 description. Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                User_Id = user.User_Id,
                longitude = -29.862501f,
                latitude = 31.04359f,
                address = "South Beach, Durban, KwaZulu-Natal",
                event_startdate = DateTime.Now.AddDays(-2),
                event_enddate = DateTime.Now.AddDays(-2).AddHours(4),
                event_createddate = DateTime.Now,
                max_attendance = 70,
                event_completed = true
            };

            Event pastEvent3 = new Event()
            {
                event_id = Guid.NewGuid().ToString(),
                event_name = "Past Event 3",
                event_descr = "Past Event 3 description. Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                User_Id = user.User_Id,
                longitude = -29.829147f,
                latitude = 31.037263f,
                address = "Bike & Bean, Durban, KwaZulu-Natal",
                event_startdate = DateTime.Now.AddDays(-3),
                event_enddate = DateTime.Now.AddDays(-3).AddHours(4),
                event_createddate = DateTime.Now,
                max_attendance = 100,
                event_completed = true
            };

            try
            {
                eventsService.Create(pastEvent1);
                eventsService.Create(pastEvent2);
                eventsService.Create(pastEvent3);
            }
            catch (AppException appException)
            {

                Console.WriteLine(appException.Message);
            }
        }

        private void CreatePetitions(User user, PetitionsSevice petitionsService)
        {
            Petition petition1 = new Petition()
            {
                petition_id = Guid.NewGuid().ToString(),
                User_Id = user.User_Id,
                name = "Petition 1",
                description = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                required_signatures = 2000,
                current_signatures = 0,
                completed = false,
                created_date = DateTime.Now
            };

            Petition petition2 = new Petition()
            {
                petition_id = Guid.NewGuid().ToString(),
                User_Id = user.User_Id,
                name = "Petition 2",
                description = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                required_signatures = 5000,
                current_signatures = 0,
                completed = false,
                created_date = DateTime.Now.AddHours(1)
            };

            Petition petition3 = new Petition()
            {
                petition_id = Guid.NewGuid().ToString(),
                User_Id = user.User_Id,
                name = "Petition 3",
                description = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                required_signatures = 1000,
                current_signatures = 0,
                completed = false,
                created_date = DateTime.Now.AddHours(2)
            };

            try
            {
                petitionsService.Create(petition1);
                petitionsService.Create(petition2);
                petitionsService.Create(petition3);
            }
            catch (AppException appException)
            {

                Console.WriteLine(appException.Message);
            }
        }

        private void CreateThreads(User user, ThreadsService threadsService)
        {
            Thread thread1 = new Thread()
            {
                thread_id = Guid.NewGuid().ToString(),
                User_Id = user.User_Id,
                thread_topic = "Thread Topic 1",
                thread_descr = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                created_date = DateTime.Now,
                thread_closed = false
            };

            Thread thread2 = new Thread()
            {
                thread_id = Guid.NewGuid().ToString(),
                User_Id = user.User_Id,
                thread_topic = "Thread Topic 2",
                thread_descr = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                created_date = DateTime.Now.AddHours(1),
                thread_closed = false
            };

            Thread thread3 = new Thread()
            {
                thread_id = Guid.NewGuid().ToString(),
                User_Id = user.User_Id,
                thread_topic = "Thread Topic 3",
                thread_descr = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum sed lebro porttitor, lacinia sem id, rutrum nunc.",
                created_date = DateTime.Now.AddHours(2),
                thread_closed = false
            };

            try
            {
                threadsService.Create(thread1);
                threadsService.Create(thread2);
                threadsService.Create(thread3);
            }
            catch (AppException appException)
            {
                Console.WriteLine(appException.Message);
            }
        }

        private void UpdateAppSetting(string key, string value)
        {
            var configJson = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);
            config[key] = value;
            var updatedConfigJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", updatedConfigJson);
        }
    }
}