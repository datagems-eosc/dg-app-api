using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Storage
{
	public static class Extensions
	{
		public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfigurationSection configurationSection)
		{
			services.ConfigurePOCO<StorageConfig>(configurationSection);
			services.AddScoped<IStorageService, StorageService>();

			return services;
		}
	}
}
