using Cite.Tools.Json;
using DataGEMS.Gateway.App.ErrorCode;
using Cite.WebTools.CurrentPrincipal.Extensions;
using Cite.WebTools.InvokerContext.Extensions;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Formatting;
using Cite.WebTools.Cors.Extensions;
using Cite.WebTools.Localization.Extensions;
using DataGEMS.Gateway.Api.Authorization;
using DataGEMS.Gateway.Api.Cache;
using DataGEMS.Gateway.Api.ForwardedHeaders;
using Cite.WebTools.HostingEnvironment.Extensions;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.Api.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.Api.LogTracking;
using Cite.WebTools.FieldSet;
using DataGEMS.Gateway.Api.HealthCheck;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.App.Accounting;

//TODO: Accounting, Validation, Logging config
namespace DataGEMS.Gateway.Api
{
    public class Startup
	{
		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			this._config = configuration;
			this._env = env;
		}

		private IConfiguration _config { get; }
		private IWebHostEnvironment _env { get; }

		public void ConfigureServices(IServiceCollection services)
		{

			services
				.AddCacheServices(this._config.GetSection("Cache:Provider")) //distributed cache
				.AddSingleton<JsonHandlingService>() //Json Handling
				.AddErrorThesaurus(this._config.GetSection("ErrorThesaurus")) //Error Thesaurus
				.AddLocalization(options => options.ResourcesPath = this._config.GetSection("Localization:Path").Get<String>()) //Localization
				.AddCurrentPrincipalResolver() //Current principal Resolver
				.AddInvokerContextResolver() //Invoker Context Resolver
				.AddEventBroker() //Event Broker
				.AddFormattingServices(this._config.GetSection("Formatting:Options"), this._config.GetSection("Formatting:Cache")) //Formatting
				.AddClaimExtractorServices(this._config.GetSection("Idp:Claims")) //Claim Extractor
				.AddAuthenticationServices(this._config.GetSection("Idp:Client")) //Authentication & JWT
				.AddCorsPolicy(this._config.GetSection("CorsPolicy")) //CORS
				.AddForwardedHeadersServices(this._config.GetSection("ForwardedHeaders")) //Forwarded Headers
				.AddAspNetCoreHostingEnvironmentResolver() //Hosting Environment
				.AddLogTrackingServices(this._config.GetSection("Tracking:Correlation"), this._config.GetSection("Tracking:Entry")) //Log tracking services
				.AddPermissionsAndPolicies(this._config.GetSection("Permissions")) //Permissions
				.AddAccountingServices(this._config.GetSection("Accounting")); //Accounting

			services
				.AddTransient<AccountBuilder>() //Account builder
			;

			HealthCheckConfig healthCheckConfig = this._config.GetSection("HealthCheck").AsHealthCheckConfig();
			services.AddFolderHealthChecks(healthCheckConfig.Folder);
			services.AddMemoryHealthChecks(healthCheckConfig.Memory);

			//Logging
			Cite.Tools.Logging.LoggingSerializerContractResolver.Instance.Configure((builder) =>
			{
				builder
					.RuntimeScannng(true);
					//.Sensitive(typeof(Cite.Tools.Http.HeaderHints), nameof(Cite.Tools.Http.HeaderHints.BearerAccessToken))
					//.Sensitive(typeof(Cite.Tools.Http.HeaderHints), nameof(Cite.Tools.Http.HeaderHints.BasicAuthenticationToken));
			}, (settings) =>
			{
				settings.Converters.Add(new Cite.Tools.Logging.StringValueEnumConverter());
			});

			//MVC
			services.AddMvcCore(options =>
			{
				options.ModelBinderProviders.Insert(0, new FieldSetModelBinderProvider());
			})
			.AddAuthorization()
			.AddNewtonsoftJson(options =>
			{
				options.SerializerSettings.Culture = System.Globalization.CultureInfo.InvariantCulture;
				options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
				options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
				options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			HealthCheckConfig healthCheckConfig = this._config.GetSection("HealthCheck").AsHealthCheckConfig();

			app
				.UseMiddleware(typeof(LogTrackingCorrelationMiddleware)) //Log Tracking Middleware
				.UseMiddleware(typeof(LogTrackingEntryMiddleware)) //Log Entry Middleware
				.UseForwardedHeaders(this._config.GetSection("ForwardedHeaders")) //Handle Forwarded Requests and preserve caller context
				.UseRequestLocalizationAndConfigure(this._config.GetSection("Localization:SupportedCultures"), this._config.GetSection("Localization:DefaultCulture")) //Request Localization
				.UseCorsPolicy(this._config.GetSection("CorsPolicy")) //CORS
				.UseMiddleware(typeof(ErrorHandlingMiddleware)) //Error Handling
				.UseRouting() //Routing
				.UseAuthentication() //Authentication
				.UseAuthorization() //Authorization
				.UseEndpoints(endpoints => //Endpoints
				{
					endpoints.MapControllers();
					if (healthCheckConfig.Endpoint?.IsEnabled ?? false) endpoints.ConfigureHealthCheckEndpoint(healthCheckConfig.Endpoint);
				})
				.BootstrapFormattingCacheInvalidationServices(); //Formatting
		}
	}
}
