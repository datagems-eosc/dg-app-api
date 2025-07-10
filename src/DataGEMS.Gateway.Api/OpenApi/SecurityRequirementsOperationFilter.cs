using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class SecurityRequirementsOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			Boolean isAuthorizedOperation = context.MethodInfo
				.GetCustomAttributes(true)
				.OfType<AuthorizeAttribute>()
				.Any();

			if (!isAuthorizedOperation) return;

			OpenApiSecurityScheme scheme = new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
			};

			operation.Security = new List<OpenApiSecurityRequirement>
			{
				new OpenApiSecurityRequirement
				{
					[scheme] = []
				}
			};
		}
	}
}
