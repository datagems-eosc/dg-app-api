using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.DatasetFileManagement
{
	public static class Extensions
	{
		public static IServiceCollection AddDatasetFileManagementServices(this IServiceCollection services, IConfigurationSection configurationSection)
		{
			services.AddTransient<IDatasetFileManagementService, DatasetFileManagementService>();

			return services;
		}
	}
}
