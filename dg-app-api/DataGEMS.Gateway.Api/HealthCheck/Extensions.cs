
namespace DataGEMS.Gateway.Api.HealthCheck
{
	public static class Extensions
	{
		public static HealthCheckConfig AsHealthCheckConfig(this IConfigurationSection healthCheckSection)
		{
			HealthCheckConfig healthCheckConfig = new HealthCheckConfig();
			healthCheckSection.Bind(healthCheckConfig);
			return healthCheckConfig;
		}

		public static IServiceCollection AddFolderHealthChecks(
			this IServiceCollection services, HealthCheckConfig.FolderConfig config,
			String[] tags = null)
		{
			Boolean isConfigured = config?.Paths?.Any() ?? false;
			if(!isConfigured) return services;

			services.AddHealthChecks()
				.AddFolder(options => config.Paths.ToList().ForEach(x => options.AddFolder(x)),
				name: "folders",
				tags: tags);

			return services;
		}

		public static IServiceCollection AddMemoryHealthChecks(
			this IServiceCollection services, HealthCheckConfig.MemoryConfig config,
			String[] tags = null)
		{
			if (config == null) return services;

			services.AddHealthChecks()
				.AddPrivateMemoryHealthCheck(config.MaxPrivateMemoryBytes, name: "privateMemory", tags: tags)
				.AddProcessAllocatedMemoryHealthCheck(Convert.ToInt32(config.MaxProcessMemoryBytes / 1024 / 1024), name: "processMemory", tags: tags)
				.AddVirtualMemorySizeHealthCheck(config.MaxVirtualMemoryBytes, name: "virtualMemory", tags: tags);

			return services;
		}

		public static IEndpointRouteBuilder ConfigureHealthCheckEndpoint(
			this IEndpointRouteBuilder endpoint, HealthCheckConfig.EndpointConfig config)
		{
			if (config == null || !config.IsEnabled) return endpoint;

			Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions theOptions = new()
			{
				AllowCachingResponses = config.AllowCaching,
				Predicate = hc => hc.Tags.Contains(config.IncludeTag),
				ResultStatusCodes =
				{
					[Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = config.HealthyStatusCode,
					[Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = config.DegradedStatusCode,
					[Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = config.UnhealthyStatusCode
				}
			};
			if (config.VerboseResponse) theOptions.ResponseWriter = DataGEMS.Gateway.Api.HealthCheck.ResponseWriter.WriteResponse;

			IEndpointConventionBuilder endpointBuilder = endpoint.MapHealthChecks(config.EndpointPath, theOptions);
			if (config.RequireHosts?.Any() ?? false) endpointBuilder.RequireHost(config.RequireHosts);

			return endpoint;
		}
	}
}
