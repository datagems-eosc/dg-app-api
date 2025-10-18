using Cite.Tools.Configuration.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public static class Extensions
	{
		public static IServiceCollection AddAAIServices(this IServiceCollection services, IConfigurationSection serviceConfigurationSection, IConfigurationSection cacheConfigurationSection)
		{
			services.ConfigurePOCO<AAIConfig>(serviceConfigurationSection);
			services.ConfigurePOCO<AAICacheConfig>(cacheConfigurationSection);
			services.AddTransient<IAAIService, AAIKeycloakService>();
			services.AddSingleton<AAICache>();

			return services;
		}

		public static IApplicationBuilder BootstrapAAICacheInvalidationServices(this IApplicationBuilder app)
		{
			AAICache cacheHandler = app.ApplicationServices.GetRequiredService<AAICache>();
			cacheHandler.RegisterListener();
			return app;
		}
	}
}
