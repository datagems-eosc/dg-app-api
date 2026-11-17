using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace DataGEMS.Gateway.Api.OpenApi
{
	/// <summary>
	/// Surfaces System.ComponentModel.DescriptionAttribute of models (types and their properties) into the generated schema,
	/// so that models living in projects without a Swashbuckle dependency can still be documented.
	/// Enums are handled by <see cref="EnumDescriptionFilter"/>.
	/// </summary>
	public class DescriptionAttributeSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (context.Type == null || context.Type.IsEnum) return;

			String typeDescription = context.Type.GetCustomAttribute<DescriptionAttribute>()?.Description;
			if (!String.IsNullOrWhiteSpace(typeDescription) && String.IsNullOrWhiteSpace(schema.Description)) schema.Description = typeDescription;

			if (schema.Properties == null || schema.Properties.Count == 0) return;

			Dictionary<String, PropertyInfo> properties = context.Type
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

			foreach (String key in schema.Properties.Keys.ToList())
			{
				if (!properties.TryGetValue(key, out PropertyInfo property)) continue;
				String description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
				if (String.IsNullOrWhiteSpace(description)) continue;

				OpenApiSchema propertySchema = schema.Properties[key];
				//GOTCHA: siblings of $ref are ignored by OpenAPI 3.0 tooling, so referenced schemas are wrapped in allOf to carry the description
				if (propertySchema.Reference != null)
				{
					schema.Properties[key] = new OpenApiSchema
					{
						AllOf = new List<OpenApiSchema> { propertySchema },
						Description = description,
						Nullable = propertySchema.Nullable,
					};
				}
				else if (String.IsNullOrWhiteSpace(propertySchema.Description)) propertySchema.Description = description;
			}
		}
	}
}
