using Cite.Tools.Configuration.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.DataManagement
{
	public static class Extensions
	{
		public static IServiceCollection AddDataManagementServices(this IServiceCollection services, IConfigurationSection dataManagementSection)
		{
			services.ConfigurePOCO<DataManagementConfig>(dataManagementSection);
			services.AddTransient<IDataManagementService, DataManagementService>();

			return services;
		}
	}
}
