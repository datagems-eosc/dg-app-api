using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Event
{
	public static class Extensions
	{
		public static IServiceCollection AddEventBroker(this IServiceCollection services)
		{
			services.AddSingleton<EventBroker>();

			return services;
		}
	}
}
