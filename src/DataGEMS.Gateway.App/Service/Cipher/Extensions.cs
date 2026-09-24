using Cite.Tools.Cipher.Extensions;
using Cite.Tools.Configuration.Extensions;
using DataGEMS.Gateway.App.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Cipher
{
	public static class Extensions
	{
		public static IServiceCollection AddCipherServices(this IServiceCollection services, IConfigurationSection cipherSection, IConfigurationSection cipherProfilesSection)
		{
			services.AddCipherServices(cipherSection);
			services.ConfigurePOCO<CipherProfiles>(cipherProfilesSection);

			return services;
		}
	}
}
