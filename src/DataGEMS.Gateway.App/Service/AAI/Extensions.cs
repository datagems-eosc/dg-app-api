using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public static class Extensions
	{
		public static IServiceCollection AddAAIServices(this IServiceCollection services, IConfigurationSection aaiConfigurationSection)
		{
			services.ConfigurePOCO<AAIConfig>(aaiConfigurationSection);
			services.AddTransient<IAAIService, AAIKeycloakService>();

			return services;
		}
	}
}
