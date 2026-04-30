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
			services.AddHttpClient("AdHocQueryClient", client => //TODO: consider renaming this to something more generic as it may be used for more than just adhoc queries in the future
			{
				client.Timeout = TimeSpan.FromMinutes(10);
			});
			services.AddScoped<ITaskOrchestratorService, TaskOrchestratorService>();

			return services;
		}
	}
}
