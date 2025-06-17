using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.ErrorCode
{
	public static class Extensions
	{
		public static IServiceCollection AddErrorThesaurus(
			this IServiceCollection services,
			IConfigurationSection errorThesaurusConfigurationSection)
		{
			services.ConfigurePOCO<ErrorThesaurus>(errorThesaurusConfigurationSection);

			return services;
		}
	}
}
