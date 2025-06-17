using Cite.Tools.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.DataManagement
{
	public static class Extensions
	{
		public static IServiceCollection AddDataManagementServices(this IServiceCollection services, IConfigurationSection dataManagementHttpSection, IConfigurationSection dataManagementLocalSection)
		{
			services.ConfigurePOCO<DataManagementHttpConfig>(dataManagementHttpSection);
			services.AddDbContext<Data.DataManagementDbContext>(options => options.UseNpgsql(dataManagementLocalSection.GetValue<String>("ConnectionStrings:DataManagementDbContext")));

			return services;
		}
	}
}
