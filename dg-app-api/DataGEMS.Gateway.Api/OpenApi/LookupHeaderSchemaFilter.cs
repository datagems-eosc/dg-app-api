using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class LookupHeaderSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			Boolean isLookupSubClass = context.Type.IsSubclassOf(typeof(Cite.Tools.Data.Query.Lookup.Header)) || context.Type == typeof(Cite.Tools.Data.Query.Lookup.Header);
			if (!isLookupSubClass) return;

			schema.Title = "Additional directives";
			schema.Description = "Additional directives for the lookup operaiton";
			schema.Nullable = true;

			schema.Properties["countAll"].Title = "Counting outside paging";
			schema.Properties["countAll"].Description = "Control if the count returned will be evaluated omitting the requested paging. If true, the full number of records mathing the predicates will be returned. If false, the count will be over the retrieved records";
			schema.Properties["countAll"].Nullable = false;

			schema.Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(new
			{
				countAll = true,
			}));
		}
	}
}
