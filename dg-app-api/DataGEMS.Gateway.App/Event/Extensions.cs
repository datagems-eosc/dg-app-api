using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
