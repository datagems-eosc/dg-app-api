using Cite.Tools.Configuration.Extensions;
using DataGEMS.Gateway.App.Service.Vocabulary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.WorkflowProcess
{
	public static class Extensions
	{
		public static IServiceCollection AddWorkflowProcessServices(this IServiceCollection services, IConfigurationSection configurationSection)
		{
			services.ConfigurePOCO<WorkflowProcessConfig>(configurationSection);
			services.AddScoped<IWorkflowProcessService, WorkflowProcessService>();
			return services;
		}
	}
}
