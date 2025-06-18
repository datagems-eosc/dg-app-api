using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class LookupFieldSetQueryStringSchemaFilter : IParameterFilter
	{
		public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
		{
			Boolean isLookupSubClass = context.ParameterInfo.ParameterType.IsSubclassOf(typeof(Cite.Tools.FieldSet.IFieldSet)) || context.ParameterInfo.ParameterType == typeof(Cite.Tools.FieldSet.IFieldSet);
			if (parameter.In != ParameterLocation.Query ||
				context.ParameterInfo is null ||
				!isLookupSubClass)
				return;

			if (context.ParameterInfo.GetCustomAttribute<LookupFieldSetQueryStringOpenApiAttribute>() != null)
			{
				parameter.Name = "f";
				parameter.Example = new OpenApiString("f=id&f=name&f=foo.id&f=foo.br.id");
			}
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class LookupFieldSetQueryStringOpenApiAttribute : Attribute
	{
	}
}
