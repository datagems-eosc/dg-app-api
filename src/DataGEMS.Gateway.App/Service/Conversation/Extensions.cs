using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.Conversation
{
	public static class Extensions
	{
		public static IServiceCollection AddConversationServices(this IServiceCollection services, IConfigurationSection conversationConfigurationSection)
		{
			services.ConfigurePOCO<ConversationConfig>(conversationConfigurationSection);

			services.AddScoped<IConversationService, ConversationService>();

			return services;
		}
	}
}
