using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator
{
	public static class Extensions
	{
		public static IServiceCollection AddTaskOrchestratorServices(this IServiceCollection services, IConfigurationSection taskOrchestratorSection)
		{
			services.ConfigurePOCO<TaskOrchestratorHttpConfig>(taskOrchestratorSection);

			String crossDatasetDiscoveryTemplatePath = taskOrchestratorSection.GetSection("CrossDatasetDiscoveryTemplatePath").Get<String>();
			string crossDatasetDiscoveryTemplateContent = File.ReadAllText(crossDatasetDiscoveryTemplatePath);
			services.AddSingleton<AnalyticalPatternTemplates>(new AnalyticalPatternTemplates() { CrossDatasetDiscoveryLookup = crossDatasetDiscoveryTemplateContent });

			services.AddScoped<ITaskOrchestratorService, TaskOrchestratorService>();

			return services;
		}
	}
}
