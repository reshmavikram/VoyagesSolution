using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using VoyagesAPIService.Infrastructure.Helper;

namespace VoyagesAPIService
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .WriteTo.File("Logs\\LoggerFile.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            try
            {
                Log.Information("Starting...");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (System.Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                Log.CloseAndFlush();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string env = "Dev";
#if Dev
            env = "Dev";
#elif Uat
            env = "Uat";
#elif Prod
            env = "Prod";
#endif

            return WebHost.CreateDefaultBuilder(args)
                  .UseStartup<Startup>()
              .UseEnvironment(env)
              .ConfigureAppConfiguration((host, builder) =>
                 host.Configuration = builder
                  .SetBasePath(host.HostingEnvironment.ContentRootPath)
                  .AddJsonFile("appsettings.json", false, true)
                  .AddJsonFile($"appsettings.{host.HostingEnvironment.EnvironmentName}.json", false, true).Build()
              )
              .ConfigureLogging((host,
              builder) =>
              {
                  builder.AddApplicationInsights(AzureVaultKey.GetVaultValue("ApplicationInsight"));// host.Configuration["ApplicationInsights:InstrumentationKey"]);
                  builder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>
                                           (typeof(Program).FullName, LogLevel.Trace);
                  builder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>
                                           (typeof(Startup).FullName, LogLevel.Trace);
              });
        }


       
    }
}