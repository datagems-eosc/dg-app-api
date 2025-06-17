using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class LookupFieldSetSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			Boolean isLookupSubClass = context.Type.IsSubclassOf(typeof(Cite.Tools.FieldSet.FieldSet)) || context.Type == typeof(Cite.Tools.FieldSet.FieldSet);
			if (!isLookupSubClass) return;

			schema.Title = "Projection Fields";
			schema.Description = "The list of field to include in the response model. Fields can be qualified with . to control nested properties";
			schema.Nullable = true;

			schema.Properties["fields"].Title = "Projection list";
			schema.Properties["fields"].Description = "The list of properties to include in the response for each returned record. Property names can be qualified with . to control nested objects";
			schema.Properties["fields"].Nullable = false;

			schema.Example = OpenApiAnyFactory.CreateFromJson(JsonConvert.SerializeObject(new
			{
				fields = new String[] { "id", "name", "foo.id", "foo.bar.id" },
			}));
		}
	}
}
