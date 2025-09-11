using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public static class Extensions
	{
		public static IServiceCollection AddAirflowServices(this IServiceCollection services, IConfigurationSection airflowConfigurationSection)
		{
			services.ConfigurePOCO<AirflowConfig>(airflowConfigurationSection);
			services.AddTransient<IAirflowAccessTokenService, AirflowAccessTokenService>();
			services.AddTransient<IAirflowExecutionService, AirflowExecutionService>();

			return services;
		}
	}
}
