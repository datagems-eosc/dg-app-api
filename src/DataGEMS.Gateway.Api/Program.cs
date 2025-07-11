using Cite.Tools.Configuration.Extensions;
using Cite.Tools.Configuration.Substitution;
using Serilog;

namespace DataGEMS.Gateway.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateWebHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webHostBuilder =>
					webHostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
					{
						IWebHostEnvironment env = hostingContext.HostingEnvironment;
						String sharedConfigPath = Path.Combine(env.ContentRootPath, "..", "Configuration");
						config
							//accounting
							.AddJsonFileInPaths("accounting.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("accounting.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"accounting.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//cache
							.AddJsonFileInPaths("cache.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("cache.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"cache.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//cors
							.AddJsonFileInPaths("cors.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("cors.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"cors.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//db
							.AddJsonFileInPaths("db.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("db.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"db.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//env
							.AddJsonFileInPaths("env.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("env.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"env.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//errors
							.AddJsonFileInPaths("errors.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("errors.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"errors.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//formatting
							.AddJsonFileInPaths("formatting.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"formatting.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"formatting.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//forwarded headers
							.AddJsonFileInPaths("forwarded-headers.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"forwarded-headers.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"forwarded-headers.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//health checks
							.AddJsonFileInPaths("health-check.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"health-check.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"health-check.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//idp claims
							.AddJsonFileInPaths("idp.claims.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("idp.claims.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"idp.claims.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//idp client
							.AddJsonFileInPaths("idp.client.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("idp.client.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"idp.client.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//localization
							.AddJsonFileInPaths("localization.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("localization.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"localization.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//logging
							.AddJsonFileInPaths("logging.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("logging.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"logging.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//log tracking
							.AddJsonFileInPaths("log-tracking.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("log-tracking.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"log-tracking.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//open api
							.AddJsonFileInPaths("open-api.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("open-api.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"open-api.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//permissions
							.AddJsonFileInPaths("permissions.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("permissions.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"permissions.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//service cross dataset discovery
							.AddJsonFileInPaths("service-cross-dataset-discovery.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("service-cross-dataset-discovery.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"service-cross-dataset-discovery.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//service data management
							.AddJsonFileInPaths("service-data-management.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("service-data-management.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"service-data-management.{env.EnvironmentName}.json", sharedConfigPath, "Configuration")
							//service in data exploration
							.AddJsonFileInPaths("service-in-data-exploration.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths("service-in-data-exploration.override.json", sharedConfigPath, "Configuration")
							.AddJsonFileInPaths($"service-in-data-exploration.{env.EnvironmentName}.json", sharedConfigPath, "Configuration");
						config.AddEnvironmentVariables("DG_GW_");
						config.EnableSubstitutions("%{", "}%");
					})
					.UseStartup<Startup>()
				)
				//Configure Serilog Logging from the configuration settings section
				.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
	}
}
