using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.UserCollection
{
	public static class Extensions
	{
		public static IServiceCollection AddUserCollectionServices(this IServiceCollection services, IConfigurationSection userCollectionConfigurationSection)
		{
			services.ConfigurePOCO<UserCollectionConfig>(userCollectionConfigurationSection);

			services
				.AddScoped<IUserCollectionService, UserCollectionService>()
				.AddScoped<IUserDatasetCollectionService, UserDatasetCollectionService>();

			return services;
		}
	}
}
