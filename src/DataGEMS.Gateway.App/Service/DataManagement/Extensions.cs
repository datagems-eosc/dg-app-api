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

		public static Service.DataManagement.Model.Dataset ToModel(this App.Service.DataManagement.Data.Dataset data)
		{
			if (data == null) return null;
			return new App.Service.DataManagement.Model.Dataset()
			{
				Id = data.Id,
				Code = data.Code,
				Name = data.Name,
				Description = data.Description,
				License = data.License,
				MimeType = data.MimeType,
				Size = data.Size,
				Url = data.Url,
				Version = data.Version,
				Headline = data.Headline,
				Keywords = data.Keywords.ParseCsv(),
				FieldOfScience = data.FieldOfScience.ParseCsv(),
				Language = data.Language.ParseCsv(),
				Country = data.Country.ParseCsv(),
				DatePublished = data.DatePublished,
				ProfileRaw = data.Profile,
			};
		}
	}
}
