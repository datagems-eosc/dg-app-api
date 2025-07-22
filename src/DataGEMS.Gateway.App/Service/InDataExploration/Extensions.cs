using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.InDataExploration
{
	public static class Extensions
	{
		public static IServiceCollection AddInDataExplorationServices(this IServiceCollection services, IConfigurationSection inDataExplorationConfigurationSection)
		{
			services.ConfigurePOCO<InDataExplorationHttpConfig>(inDataExplorationConfigurationSection);

			services.AddTransient<IInDataExplorationService, InDataExplorationHttpService>();

			return services;
		}
	}
}
