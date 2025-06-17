using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class LookupOrderSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			Boolean isLookupSubClass = context.Type.IsSubclassOf(typeof(Cite.Tools.Data.Query.Ordering)) || context.Type == typeof(Cite.Tools.Data.Query.Ordering);
			if (!isLookupSubClass) return;

			schema.Title = "Ordering directives";
			schema.Description = "Ordering directives to be applied for the returned models. If paging is set, ordering must be set also";
			schema.Nullable = true;

			schema.Properties["items"].Title = "Ordering clause";
			schema.Properties["items"].Description = "The ordering to be applied for the returned records. Multiple fields can be set. Prepending + applies ascending ordering while - descending";
			schema.Properties["items"].Nullable = false;

			schema.Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(new
			{
				items = new String[] { "+name", "-id" },
			}));
		}
	}
}
