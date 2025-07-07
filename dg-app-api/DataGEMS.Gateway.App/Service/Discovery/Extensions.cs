using Cite.Tools.Configuration.Extensions;
using DataGEMS.Gateway.App.AccessToken;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Discovery
{
	public static class Extensions
	{
		public static IServiceCollection AddCrossDatasetDiscoveryServices(this IServiceCollection services, IConfigurationSection crossDatasetDiscoveryConfigurationSection)
		{
			services.ConfigurePOCO<CrossDatasetDiscoveryHttpConfig>(crossDatasetDiscoveryConfigurationSection);

			services.AddTransient<ICrossDatasetDiscoveryService, CrossDatasetDiscoveryHttpService>();

			return services;
		}
	}
}
