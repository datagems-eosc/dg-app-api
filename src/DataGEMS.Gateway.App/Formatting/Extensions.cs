using Cite.Tools.Configuration.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Formatting
{
	public static class Extensions
	{
		public static IServiceCollection AddFormattingServices(
			this IServiceCollection services,
			IConfigurationSection configurationSection,
			IConfigurationSection cacheConfigurationSection)
		{
			services.AddScoped<IFormattingService, FormattingService>();
			services.ConfigurePOCO<FormattingServiceConfig>(configurationSection);
			services.ConfigurePOCO<FormattingCacheConfig>(cacheConfigurationSection);
			services.AddSingleton<FormattingCache>();

			return services;
		}

		public static IApplicationBuilder BootstrapFormattingCacheInvalidationServices(this IApplicationBuilder app)
		{
			FormattingCache cacheHandler = app.ApplicationServices.GetRequiredService<FormattingCache>();
			cacheHandler.RegisterListener();
			return app;
		}
	}
}
