using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.DatasetPackaging
{
	public static class Extensions
	{
		public static IServiceCollection AddDatasetPackagingServices(this IServiceCollection services, IConfigurationSection datasetPackagingConfigurationSection)
		{
			services.ConfigurePOCO<DatasetPackagingHttpConfig>(datasetPackagingConfigurationSection);

			services.AddTransient<IDatasetPackagingService, DatasetPackagingHttpService>();
			return services;
		}
	}
}
