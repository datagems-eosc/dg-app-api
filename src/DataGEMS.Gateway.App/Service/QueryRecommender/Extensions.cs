using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.QueryRecommender
{
	public static class Extensions
	{
		public static IServiceCollection AddQueryRecommenderServices(this IServiceCollection services, IConfigurationSection configurationSection)
		{
			services.ConfigurePOCO<QueryRecommenderHttpConfig>(configurationSection);

			services.AddTransient<IQueryRecommenderHttpService, QueryRecommenderHttpService>();

			return services;
		}
	}
}
