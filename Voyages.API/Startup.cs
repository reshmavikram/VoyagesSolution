using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;
using VoyagesAPIService.Filter;
using VoyagesAPIService.Infrastructure.Helper;
using VoyagesAPIService.Infrastructure.Middlewares;
using VoyagesAPIService.Infrastructure.Services;
using VoyagesAPIService.Infrastructure.Services.Interfaces;

namespace VoyagesAPIService
{
    public class Startup
    {
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, IHostingEnvironment environment, ILogger<Startup> logger)
        {
            Configuration = configuration;
            StaticConfig = configuration;
            Environment = environment;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration StaticConfig { get; private set; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(opts => opts.UseSqlServer("Data Source=DESKTOP-ATHA8AK\\SQLEXPRESS;Initial Catalog=VPS_DB_New_Logic;Integrated Security=True", m => m.MigrationsAssembly("Masters.API")));
            services.AddApplicationInsightsTelemetry();
            _logger.LogInformation("Logging from ConfigureServices.");

         /*   services.AddDbContext<DatabaseContext>(opts => opts.UseSqlServer("Server=tcp:" + AzureVaultKey.GetVaultValue("DbServer") + "," + AzureVaultKey.GetVaultValue("DbServerPort") + ";Initial Catalog=" + AzureVaultKey.GetVaultValue("DbSchema") + "; "
                    + " Persist Security Info=False;User ID=" + AzureVaultKey.GetVaultValue("DbUser") + ";Password=" + AzureVaultKey.GetVaultValue("DbPass") + ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;", m => m.MigrationsAssembly("Masters.API")));
            services.AddApplicationInsightsTelemetry();
            _logger.LogInformation("Logging from ConfigureServices.");*/
            services.AddDistributedRedisCache(r => { r.Configuration = "" + AzureVaultKey.GetVaultValue("RedisCacheServer") + ":" + AzureVaultKey.GetVaultValue("CachePort") + ",password=" + AzureVaultKey.GetVaultValue("RedisCachePass") + ",abortConnect=false,ssl=true"; });
         
            services.AddCors();
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders());
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Scorpio Users API", Version = "v1" });
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Enter Auth Token with  word 'Bearer' following by space  ",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                    },
                    new List < string > ()
                }
                });
            });
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Info { Title = "Scorpio Voyages API", Version = "v1" });
            //    c.AddSecurityDefinition("Bearer",
            //        new ApiKeyScheme
            //        {
            //            In = "header",
            //            Description = "Enter Auth Token with  word 'Bearer' following by space  ",
            //            Name = "Authorization",
            //            Type = "apiKey"
            //        });
            //    c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
            //        { "Bearer", Enumerable.Empty<string>() },
            //                });
            //});

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            //    .AddJsonOptions(
            //options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddControllersWithViews()
   .AddNewtonsoftJson(options =>
   options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
   );

            var tokenEndpoint = AzureVaultKey.GetVaultValue("AuthServertokenEndpoint");
            services.AddAuthentication("Bearer").AddIdentityServerAuthentication(option =>
            {
                option.Authority = tokenEndpoint;
                option.RequireHttpsMetadata = false;
                option.ApiName = "voyages"; //This is the resourceAPI that we defined in the Config.cs in the LoginWeb project above. In order to work it has to be named equal.
            });

            services.AddTransient<IVoyagesService, VoyagesService>();
            services.AddTransient<IFleetViewService, FleetViewServices>();
            services.AddScoped<AuthServer>();
            services.AddScoped<UserContext>(f =>
            {
                UserContext result = new UserContext();
                IHttpContextAccessor context = f.GetService<IHttpContextAccessor>();
                var routeData = context.HttpContext.GetRouteData();
                result.UserId = context.HttpContext.Request.Query.Where(x=>x.Key== "userId").Select(x => x.Value).FirstOrDefault();//routeData?.Values["UserId"]?.ToString();
                return result;
            });
            AzureVaultKey.SetVaultValue();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Dev")
            {
                _logger.LogInformation("Configuring for Development environment");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                _logger.LogInformation("Configuring for Production environment");
                app.UseHsts();
            }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(new string[] { "Status", "Message" }));
            app.UseHttpsRedirection();
            app.UseDatabaseErrorPage();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Scorpio Voyages API");
            });
        }
    }
}
