using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.InDatasetDiscovery
{
	public static class Extensions
	{
		public static IServiceCollection AddInDatasetDiscoveryServices(this IServiceCollection services, IConfigurationSection inDatasetDiscoveryConfigurationSection)
		{
			services.ConfigurePOCO<InDatasetDiscoveryHttpConfig>(inDatasetDiscoveryConfigurationSection);

			services.AddTransient<IInDatasetDiscoveryService, InDatasetDiscoveryHttpService>();

			return services;
		}
	}
}
