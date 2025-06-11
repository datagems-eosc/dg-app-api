using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.LogTracking
{
	public static class Extensions
	{
		public static IServiceCollection AddLogTrackingServices(
			this IServiceCollection services,
			IConfigurationSection logTrackingCorrelationConfigurationSection,
			IConfigurationSection logTrackingEntryConfigurationSection)
		{
			services.ConfigurePOCO<LogTrackingCorrelationConfig>(logTrackingCorrelationConfigurationSection);
			services.AddScoped<LogCorrelationScope>();

			services.ConfigurePOCO<LogTrackingEntryConfig>(logTrackingEntryConfigurationSection);

			return services;
		}
	}
}
