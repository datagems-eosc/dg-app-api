using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Globalization;

namespace DataGEMS.Gateway.App.Common
{
	public static class AnalyticalPatternLabels
	{
		public static class Node
		{
			public const String Dataset = "sc:Dataset";
			public const String Data = "Data";
			public const String RecordSet = "cr:RecordSet";
			public const String Field = "cr:Field";
			public const String FileObject = "cr:FileObject";
			public const String FileSet = "cr:FileSet";
			public const String DatabaseConnection = "dg:DatabaseConnection";
		}

		public static class Edge
		{
			public const String Distribution = "distribution";
			public const String RecordSet = "recordSet";
			public const String Field = "field";
			public const String SourceFileObject = "source/fileObject";
			public const String ContainedIn = "containedIn";
		}
	}

	/// <summary>
	/// Tolerant readers for <see cref="AnalyticalPatternNode.Properties"/>. After Newtonsoft deserialization into
	/// Dictionary&lt;String, Object&gt;, values may be CLR primitives, DateTime (date-like strings), JArray / JObject,
	/// or JSON encoded strings, so every reader accepts all of these and returns null instead of throwing.
	/// </summary>
	public static class AnalyticalPatternExtensions
	{
		public static Boolean HasLabel(this AnalyticalPatternNode node, String label)
		{
			return node?.Labels != null && node.Labels.Contains(label);
		}

		public static Boolean HasLabel(this AnalyticalPatternEdge edge, String label)
		{
			return edge?.Labels != null && edge.Labels.Contains(label);
		}

		public static Object Property(this AnalyticalPatternNode node, String key)
		{
			if (node?.Properties == null || key == null) return null;
			return node.Properties.TryGetValue(key, out Object value) ? value : null;
		}

		public static String PropertyAsString(this AnalyticalPatternNode node, String key) => AsString(node.Property(key));
		public static List<String> PropertyAsStringList(this AnalyticalPatternNode node, String key) => AsStringList(node.Property(key));
		public static Double? PropertyAsDouble(this AnalyticalPatternNode node, String key) => AsDouble(node.Property(key));
		public static long? PropertyAsLong(this AnalyticalPatternNode node, String key) => AsLong(node.Property(key));
		public static DateTime? PropertyAsDateTime(this AnalyticalPatternNode node, String key) => AsDateTime(node.Property(key));
		public static DateOnly? PropertyAsDateOnly(this AnalyticalPatternNode node, String key) => AsDateOnly(node.Property(key));
		public static Guid? PropertyAsGuid(this AnalyticalPatternNode node, String key) => AsGuid(node.Property(key));
		public static JToken PropertyAsJToken(this AnalyticalPatternNode node, String key) => AsJToken(node.Property(key));

		public static String AsString(Object value)
		{
			switch (value)
			{
				case null: return null;
				case String s: return s;
				case JValue jv: return AsString(jv.Value);
				case JToken jt: return jt.ToString();
				case DateTime dt: return dt.TimeOfDay == TimeSpan.Zero && dt.Kind == DateTimeKind.Unspecified ? dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : dt.ToString("o", CultureInfo.InvariantCulture);
				case DateTimeOffset dto: return dto.ToString("o", CultureInfo.InvariantCulture);
				case IFormattable f: return f.ToString(null, CultureInfo.InvariantCulture);
				default: return value.ToString();
			}
		}

		public static List<String> AsStringList(Object value)
		{
			switch (value)
			{
				case null: return null;
				case String s: return [s];
				case JArray ja: return ja.Select(x => AsString(x)).ToList();
				case JValue jv: return AsStringList(jv.Value);
				case IEnumerable e: return e.Cast<Object>().Select(AsString).ToList();
				default: return [AsString(value)];
			}
		}

		public static Double? AsDouble(Object value)
		{
			Double? result = value switch
			{
				null => null,
				JValue jv => AsDouble(jv.Value),
				Double d => d,
				Single f => f,
				Decimal m => (Double)m,
				long l => l,
				int i => i,
				short sh => sh,
				System.Numerics.BigInteger bi => (Double)bi,
				String s => Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out Double parsed) ? parsed : null,
				_ => null,
			};
			//NaN / Infinity can come from python json.dumps and are not meaningful downstream
			if (result.HasValue && (Double.IsNaN(result.Value) || Double.IsInfinity(result.Value))) return null;
			return result;
		}

		public static long? AsLong(Object value)
		{
			switch (value)
			{
				case null: return null;
				case JValue jv: return AsLong(jv.Value);
				case long l: return l;
				case int i: return i;
				case short s: return s;
				case String str when long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed): return parsed;
				default:
					{
						Double? d = AsDouble(value);
						if (!d.HasValue || d.Value > long.MaxValue || d.Value < long.MinValue) return null;
						return (long)Math.Round(d.Value);
					}
			}
		}

		public static DateTime? AsDateTime(Object value)
		{
			switch (value)
			{
				case null: return null;
				case JValue jv: return AsDateTime(jv.Value);
				case DateTime dt: return dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : dt;
				case DateTimeOffset dto: return dto.UtcDateTime;
				case String s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsed): return parsed.UtcDateTime;
				default: return null;
			}
		}

		public static DateOnly? AsDateOnly(Object value)
		{
			switch (value)
			{
				case null: return null;
				case JValue jv: return AsDateOnly(jv.Value);
				case DateOnly d: return d;
				case DateTime dt: return DateOnly.FromDateTime(dt);
				case DateTimeOffset dto: return DateOnly.FromDateTime(dto.DateTime);
				case String s when DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsed): return parsed;
				case String s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsed): return DateOnly.FromDateTime(parsed.DateTime);
				default: return null;
			}
		}

		public static Guid? AsGuid(Object value)
		{
			switch (value)
			{
				case null: return null;
				case Guid g: return g;
				case JValue jv: return AsGuid(jv.Value);
				case String s when Guid.TryParse(s, out Guid parsed): return parsed;
				default: return null;
			}
		}

		/// <summary>
		/// Returns the value as a JToken. Strings holding JSON (e.g. "examples") are parsed;
		/// strings that are not JSON are returned as a JValue.
		/// </summary>
		public static JToken AsJToken(Object value)
		{
			switch (value)
			{
				case null: return null;
				case JValue jv when jv.Value is String s: return AsJToken(s);
				case JToken jt: return jt;
				case String s:
					{
						String trimmed = s.Trim();
						if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
						{
							try { return JToken.Parse(trimmed); }
							catch (JsonException) { }
						}
						return new JValue(s);
					}
				default:
					try { return JToken.FromObject(value); }
					catch (JsonException) { return null; }
			}
		}

		/// <summary>
		/// Converts JToken trees to plain CLR values (Dictionary / List / primitives) so they serialize cleanly.
		/// </summary>
		public static Object ToPlain(Object value)
		{
			switch (value)
			{
				case null: return null;
				case JValue jv: return ToPlain(jv.Value);
				case JArray ja: return ja.Select(x => ToPlain(x)).ToList();
				case JObject jo: return jo.Properties().ToDictionary(x => x.Name, x => ToPlain(x.Value));
				case Double d when Double.IsNaN(d) || Double.IsInfinity(d): return null;
				case Single f when Single.IsNaN(f) || Single.IsInfinity(f): return null;
				default: return value;
			}
		}

		public static List<Object> ToPlainList(JToken token)
		{
			if (token == null || token.Type == JTokenType.Null) return null;
			if (token is JArray ja) return ja.Select(x => ToPlain(x)).ToList();
			return [ToPlain(token)];
		}
	}
}
