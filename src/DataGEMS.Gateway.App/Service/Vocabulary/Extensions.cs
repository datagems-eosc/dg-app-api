using System.Globalization;
using System.Runtime.CompilerServices;
using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Vocabulary
{
	public static class Extensions
	{
		public static IServiceCollection AddVocabularyServices(
			this IServiceCollection services, 
			IConfigurationSection fieldsOfScienceConfigurationSection,
			IConfiguration licenseConfigurationSection
			)
		{
			services.ConfigurePOCO<FieldsOfScienceVocabulary>(fieldsOfScienceConfigurationSection);
			services.ConfigurePOCO<LicenseVocabulary>(licenseConfigurationSection);

			return services;
		}
	}
}
