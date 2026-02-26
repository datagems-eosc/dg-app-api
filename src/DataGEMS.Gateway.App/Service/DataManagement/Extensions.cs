using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
    public static class Extensions
    {
        public static IServiceCollection AddDataManagementServices(this IServiceCollection services, IConfigurationSection dataManagementSection)
        {
            services.ConfigurePOCO<DataManagementHttpConfig>(dataManagementSection);
			services.AddScoped<IDataManagementService, DataManagementService>();

			return services;
        }
	}
}
