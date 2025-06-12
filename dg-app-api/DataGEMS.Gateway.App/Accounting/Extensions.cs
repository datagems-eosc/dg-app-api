using Cite.Tools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Accounting
{
	public static class Extensions
	{
		public static IServiceCollection AddAccountingServices(this IServiceCollection services, IConfigurationSection configurationSection)
		{
			services.ConfigurePOCO<AccountingLogConfig>(configurationSection);
			services.AddScoped<IAccountingService, AccountingLogService>();
			return services;
		}

		public static String AsAccountable(this KnownResources knownResource)
		{
			return knownResource.ToString();
		}
	}
}
