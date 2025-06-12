using Cite.Tools.Configuration.Extensions;
using Cite.Tools.DI;
using Cite.Tools.DI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Query
{
	public static class Extensions
	{
		public static IServiceCollection AddQueriesAndFactory(this IServiceCollection services, Type registerImplementationsOfType, params Type[] assemblyContainingType)
		{
			HashSet<Type> registeredQueries = new HashSet<Type>();
			foreach (Type tt in assemblyContainingType)
			{
				services.AddMyTransientTypes(config => config.RegisterFromAssemblyContaining(tt).RegisterTarget(registerImplementationsOfType), pair => { registeredQueries.Add(pair.ImplementationType); });
			}
			services.ConfigurePOCO(new QueryFactory.QueryFactoryConfig(registeredQueries));
			services.AddScoped<QueryFactory>();

			return services;
		}
	}
}
