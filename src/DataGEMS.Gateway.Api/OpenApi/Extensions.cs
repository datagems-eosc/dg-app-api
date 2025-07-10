using Microsoft.OpenApi.Models;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public static class Extensions
	{
		public static OpenApiConfig AsOpenApiConfig(this IConfigurationSection openApiSection)
		{
			OpenApiConfig openApiConfig = new OpenApiConfig();
			openApiSection.Bind(openApiConfig);
			return openApiConfig;
		}

		public static IServiceCollection AddOpenApiServices(this IServiceCollection services, IConfigurationSection openApiSection)
		{
			OpenApiConfig openApiConfig = openApiSection.AsOpenApiConfig();

			services
				.AddSwaggerGen(options =>
				{
					options.SwaggerDoc("v1", new OpenApiInfo
					{
						Version = openApiConfig.Version,
						Title = openApiConfig.Title,
						Description = openApiConfig.Description,
						TermsOfService = openApiConfig.Terms.Url,
						Contact = new OpenApiContact
						{
							Name = openApiConfig.Contact.Name,
							Url = openApiConfig.Contact.Url
						},
						License = new OpenApiLicense
						{
							Name = openApiConfig.License.Name,
							Url = openApiConfig.License.Url
						}
					});
					options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
					{
						Type = SecuritySchemeType.OAuth2,
						Flows = new OpenApiOAuthFlows
						{
							AuthorizationCode = new OpenApiOAuthFlow
							{
								AuthorizationUrl = openApiConfig.OAuth2.AuthorizationUrl,
								TokenUrl = openApiConfig.OAuth2.TokenUrl,
								Scopes = openApiConfig.OAuth2.Scopes
							}
						}
					});
					if(!string.IsNullOrEmpty(openApiConfig.BasePath)) options.AddServer(new OpenApiServer { Url = openApiConfig.BasePath });
					options.EnableAnnotations();
					options.SchemaFilter<LookupPageSchemaFilter>();
					options.SchemaFilter<LookupOrderSchemaFilter>();
					options.SchemaFilter<LookupHeaderSchemaFilter>();
					options.SchemaFilter<LookupFieldSetSchemaFilter>();
					options.SchemaFilter<EnumDescriptionFilter>();
					options.OperationFilter<SecurityRequirementsOperationFilter>();
					options.ParameterFilter<LookupFieldSetQueryStringSchemaFilter>();
				})
				.AddSwaggerGenNewtonsoftSupport();

			return services;
		}

		public static IApplicationBuilder ConfigureUseSwagger(this IApplicationBuilder app, IConfigurationSection openApiSection, String environmentName)
		{
			OpenApiConfig openApiConfig = openApiSection.AsOpenApiConfig();

			Boolean isEnvironmentConfigured = openApiConfig.Environments?.Contains(environmentName) ?? false;
			if (!isEnvironmentConfigured) return app;

			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", openApiConfig.Title);
				options.OAuth2RedirectUrl("/swagger/oauth2-redirect.html");
				options.OAuthClientId(openApiConfig.OAuth2.ClientId);
				options.OAuthAppName(openApiConfig.OAuth2.ClientName);
				if (openApiConfig.OAuth2.UsePkce) options.OAuthUsePkce();
			});

			return app;
		}
	}
}
