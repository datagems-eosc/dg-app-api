using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Vocabulary
{
	public static class Extensions
	{
		public static IServiceCollection AddVocabularyServices(
			this IServiceCollection services, 
			IConfigurationSection fieldsOfScienceConfigurationSection)
		{
			services.ConfigurePOCO<FieldsOfScienceVocabulary>(fieldsOfScienceConfigurationSection);

			return services;
		}
	}
}
