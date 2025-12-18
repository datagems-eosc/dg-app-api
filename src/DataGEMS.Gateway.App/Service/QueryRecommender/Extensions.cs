using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.QueryRecommender
{
	public static class Extensions
	{
		public static IServiceCollection AddQueryRecommenderServices(this IServiceCollection services, IConfigurationSection queryRecommenderConfigurationSection)
		{
			services.ConfigurePOCO<QueryRecommenderHttpConfig>(queryRecommenderConfigurationSection);

			services.AddTransient<IQueryRecommenderService, QueryRecommenderHttpService>();

			return services;
		}
	}
}
