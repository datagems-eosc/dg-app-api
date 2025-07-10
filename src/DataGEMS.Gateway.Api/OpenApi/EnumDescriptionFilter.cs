using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace DataGEMS.Gateway.Api.OpenApi
{
	public class EnumDescriptionFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (!context.Type.IsEnum) return;

			OpenApiArray names = new OpenApiArray();
			OpenApiArray descriptions = new OpenApiArray();

			foreach (FieldInfo field in context.Type.GetFields(BindingFlags.Public | BindingFlags.Static))
			{

				int value = 0;
				try { value = Convert.ToInt32(Enum.Parse(context.Type, field.Name)); }catch(System.Exception) { value = 0; }
				names.Add(new OpenApiString($"{value} - {field.Name}"));
				String desc = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name; 
				descriptions.Add(new OpenApiString($"{value} - {desc}"));
			}

			// Attach vendor extensions the de‑facto standard way
			schema.Extensions["x-enum-varnames"] = names;
			schema.Extensions["x-enum-descriptions"] = descriptions;

			// Append a quick map to the schema description
			String map = string.Join(", ", names.Zip(descriptions, (n, d) => $"{(n as OpenApiString)?.Value} = {(d as OpenApiString)?.Value}"));
			schema.Description = (schema.Description ?? string.Empty) + $"  ({map})";
		}
	}
}
