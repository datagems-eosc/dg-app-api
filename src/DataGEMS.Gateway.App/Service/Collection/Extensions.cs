using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Collection
{
	public static class Extensions
	{
		public static IServiceCollection AddCollectionServices(this IServiceCollection services)
		{
			services.AddScoped<ICollectionService, CollectionService>();

			return services;
		}
	}
}
