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
using Serilog;
using Cite.Tools.Data.Censor.Extensions;
using DataGEMS.Gateway.App.DataManagement;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.Api.AccessToken;
using Cite.Tools.Data.Query.Extensions;
using Cite.Tools.Data.Builder.Extensions;
using Cite.Tools.Validation.Extensions;
using DataGEMS.Gateway.Api.OpenApi;
using Microsoft.EntityFrameworkCore;
using DataGEMS.Gateway.App.Service.UserCollection;
using DataGEMS.Gateway.Api.Transaction;
using Cite.Tools.Data.Deleter.Extensions;

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
				.AddHttpClient() //HttpClient for outgoing http calls
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
				.AddAuthorizationContentResolverServices() //Authorization Content Resolver
				.AddAccountingServices(this._config.GetSection("Accounting")) //Accounting
				.AddAccessTokenServices(); //Access token management services

			services
				.AddCensorsAndFactory(typeof(Cite.Tools.Data.Censor.ICensor), typeof(DataGEMS.Gateway.App.AssemblyHandle)) //Censors
				.AddQueriesAndFactory(typeof(Cite.Tools.Data.Query.IQuery), typeof(DataGEMS.Gateway.App.AssemblyHandle)) //Queries
				.AddBuildersAndFactory(typeof(Cite.Tools.Data.Builder.IBuilder), typeof(DataGEMS.Gateway.App.AssemblyHandle)) //Builders
				.AddTransient<AccountBuilder>() //Account builder
				.AddValidatorsAndFactory(typeof(Cite.Tools.Validation.IValidator), typeof(DataGEMS.Gateway.App.AssemblyHandle), typeof(DataGEMS.Gateway.Api.AssemblyHandle)) //Validators
				.AddDeletersAndFactory(typeof(Cite.Tools.Data.Deleter.IDeleter), typeof(DataGEMS.Gateway.App.AssemblyHandle)) //Deleters
				.AddDbContext<DataGEMS.Gateway.App.Data.AppDbContext>(options => options.UseNpgsql(this._config.GetValue<String>("DB:ConnectionStrings:AppDbContext"))) //DbContext
				.AddScoped<AppTransactionFilter>() //Transaction Filter
			;

			services
				.AddDataManagementServices(this._config.GetSection("DataManagementService:Http"), this._config.GetSection("DataManagementService:Local")) //Data Management API
			;

			services
				.AddScoped<IUserCollectionService, UserCollectionService>()
				.AddScoped<IUserDatasetCollectionService, UserDatasetCollectionService>()
			;


			HealthCheckConfig healthCheckConfig = this._config.GetSection("HealthCheck").AsHealthCheckConfig();
			services.AddFolderHealthChecks(healthCheckConfig.Folder);
			services.AddMemoryHealthChecks(healthCheckConfig.Memory);
			services.AddDbHealthChecks<DataGEMS.Gateway.App.Data.AppDbContext>();

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
			})
			.AddApiExplorer(); //needed because of Swashbuckle

			services.AddOpenApiServices(this._config.GetSection("OpenApi"));
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			HealthCheckConfig healthCheckConfig = this._config.GetSection("HealthCheck").AsHealthCheckConfig();

			app
				.UseMiddleware(typeof(LogTrackingCorrelationMiddleware)) //Log Tracking Middleware
				.UseSerilogRequestLogging() //Aggregated request info logging
				.UseForwardedHeaders(this._config.GetSection("ForwardedHeaders")) //Handle Forwarded Requests and preserve caller context
				.UseRequestLocalizationAndConfigure(this._config.GetSection("Localization:SupportedCultures"), this._config.GetSection("Localization:DefaultCulture")) //Request Localization
				.UseCorsPolicy(this._config.GetSection("CorsPolicy")) //CORS
				.UseMiddleware(typeof(ErrorHandlingMiddleware)) //Error Handling
				.UseRouting() //Routing
				.UseAuthentication() //Authentication
				.UseAuthorization() //Authorization
				.UseMiddleware(typeof(LogTrackingEntryMiddleware)) //Log Entry Middleware
				.UseMiddleware(typeof(AccessTokenInterceptMiddleware)) //Bearer Authorization AccessToken interception
				.UseMiddleware(typeof(UserSyncMiddleware)) //User sync to store and update request user
				.UseEndpoints(endpoints => //Endpoints
				{
					endpoints.MapControllers();
					if (healthCheckConfig.Endpoint?.IsEnabled ?? false) endpoints.ConfigureHealthCheckEndpoint(healthCheckConfig.Endpoint);
				})
				.ConfigureUseSwagger(this._config.GetSection("OpenApi"), env.EnvironmentName)
				.BootstrapFormattingCacheInvalidationServices(); //Formatting
		}
	}
}
