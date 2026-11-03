using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.IntentDisambiguation
{
	public static class Extensions
	{
		public static IServiceCollection AddIntentDisambiguationServices(this IServiceCollection services, IConfigurationSection intentDisambiguationConfigurationSection)
		{
			services.ConfigurePOCO<IntentDisambiguationHttpConfig>(intentDisambiguationConfigurationSection);

			services.AddTransient<IIntentDisambiguationService, IntentDisambiguationHttpService>();

			return services;
		}
	}
}
