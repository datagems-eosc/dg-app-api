using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.DatasetRecommender
{
	public static class Extensions
	{
		public static IServiceCollection AddDatasetRecommenderServices(this IServiceCollection services, IConfigurationSection datasetRecommenderConfigurationSection)
		{
			services.ConfigurePOCO<DatasetRecommenderHttpConfig>(datasetRecommenderConfigurationSection);

			services.AddTransient<IDatasetRecommenderService, DatasetRecommenderHttpService>();
			return services;
		}
	}
}
