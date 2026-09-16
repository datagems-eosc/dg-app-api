using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.DatasetLinking
{
	public static class Extensions
	{
		public static IServiceCollection AddDatasetLinkingServices(this IServiceCollection services, IConfigurationSection datasetLinkingConfigurationSection)
		{
			services.ConfigurePOCO<DatasetLinkingHttpConfig>(datasetLinkingConfigurationSection);

			services.AddTransient<IDatasetLinkingService, DatasetLinkingHttpService>();

			return services;
		}
	}
}
