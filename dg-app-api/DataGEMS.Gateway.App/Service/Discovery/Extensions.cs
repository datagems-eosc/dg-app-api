using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Discovery
{
	public static class Extensions
	{
		public static IServiceCollection AddCrossDatasetDiscoveryServices(this IServiceCollection services, IConfigurationSection crossDatasetDiscoveryConfigurationSection)
		{
			services.ConfigurePOCO<CrossDatasetDiscoveryHttpConfig>(crossDatasetDiscoveryConfigurationSection);

			services.AddTransient<ICrossDatasetDiscoveryService, CrossDatasetDiscoveryHttpService>();

			return services;
		}
	}
}
