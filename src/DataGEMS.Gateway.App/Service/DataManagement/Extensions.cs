using Cite.Tools.Configuration.Extensions;
using DataGEMS.Gateway.App.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.DataManagement
{
    public static class Extensions
    {
        public static IServiceCollection AddDataManagementServices(this IServiceCollection services, IConfigurationSection dataManagementHttpSection, IConfigurationSection dataManagementLocalSection)
        {
            services.ConfigurePOCO<DataManagementHttpConfig>(dataManagementHttpSection);
            services.AddDbContext<Data.DataManagementDbContext>(options => options.UseNpgsql(dataManagementLocalSection.GetValue<string>("ConnectionStrings:DataManagementDbContext")));
            services.AddScoped<ICollectionService, CollectionLocalService>();
			services.AddScoped<IDatasetService, DatasetLocalService>();

			return services;
        }

		public static Service.DataManagement.Model.Collection ToModel(this App.Service.DataManagement.Data.Collection data)
		{
			if (data == null) return null;
			return new App.Service.DataManagement.Model.Collection()
			{
				Id = data.Id,
				Code = data.Code,
				Name = data.Name,
			};
		}
	}
}
