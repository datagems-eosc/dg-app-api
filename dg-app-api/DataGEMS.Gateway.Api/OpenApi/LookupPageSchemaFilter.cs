using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class LookupPageSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			Boolean isLookupSubClass = context.Type.IsSubclassOf(typeof(Cite.Tools.Data.Query.Paging)) || context.Type == typeof(Cite.Tools.Data.Query.Paging);
			if (!isLookupSubClass) return;

			schema.Title = "Paging directives";
			schema.Description = "Paging directives to be applied for the returned models. If paging is set, ordering must be set also";
			schema.Nullable = true;

			schema.Properties["offset"].Title = "Record offset";
			schema.Properties["offset"].Description = "Offset to start retrieving records from. Starts from 0";
			schema.Properties["offset"].Nullable = false;

			schema.Properties["size"].Title = "Record size";
			schema.Properties["size"].Description = "Number of records to retrieve";
			schema.Properties["size"].Nullable = false;

			schema.Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(new
			{
				offset = 0,
				size = 10
			}));
		}
	}
}
