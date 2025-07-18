using Cite.Tools.Common.Extensions;
using Cite.Tools.FieldSet;
using Cite.Tools.Time;
using System.Security.Cryptography;
using System.Text;

namespace DataGEMS.Gateway.App.Common
{
	public static class Extensions
	{
		public static List<String> ParseCsv(this String value)
		{
			if(String.IsNullOrEmpty(value)) return null;
			String[] values = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			return values.ToList();
		}

		public static String ToETag(this DateTime updatedAt)
		{
			return updatedAt.ToEpoch().ToString();
		}

		public static String ToSha256(this String input)
		{
			if (String.IsNullOrEmpty(input)) return String.Empty;
			using (var sha = SHA256.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(input);
				var hash = sha.ComputeHash(bytes);

				return Convert.ToBase64String(hash);
			}
		}

		public static Boolean IsNotNullButEmpty<T>(this IEnumerable<T> enumerable)
		{
			if(enumerable == null) return false;
			return !enumerable.Any();
		}

		public static IFieldSet ExtractNonPrefixed(this IFieldSet fieldSet, String qualifier = ".")
		{
			if (fieldSet == null) return null;

			List<String> nonQualified = fieldSet.Fields.Where(x => !x.Contains(qualifier)).ToList();
			return new FieldSet(nonQualified);
		}

		public static IFieldSet MergeAsPrefixed(this IFieldSet fieldSet, IFieldSet other, String prefix, String qualifier = ".")
		{
			if (other == null || other.IsEmpty()) return fieldSet;

			List<String> qualifiedOthers = other.Fields.Select(x => new String[] { prefix, x }.AsIndexer(qualifier)).ToList();
			IFieldSet merged = fieldSet.Merge(new FieldSet(qualifiedOthers));
			return merged;
		}

		public static bool CensoredAsUnauthorized(this IFieldSet requested, IFieldSet censored, String qualifier = ".")
		{
			Boolean isRequestedNonEmpty = requested != null && !requested.IsEmpty();
			Boolean isCensoredEmpty = censored == null || censored.IsEmpty();

			Boolean hasRequestedTopLevel = requested != null && !requested.IsEmpty() && requested.Fields.Any(x => !x.Contains(qualifier));
			Boolean hasCensoredTopLevel = censored != null && !censored.IsEmpty() && censored.Fields.Any(x => !x.Contains(qualifier));

			Boolean isTopLevelFiltered = hasRequestedTopLevel && !hasCensoredTopLevel;

			return (isRequestedNonEmpty && isCensoredEmpty) || isTopLevelFiltered;
		}

		public static List<String> ReduceToAssignedPermissions(this IEnumerable<String> requested, IEnumerable<String> assigned)
		{
			if (assigned == null || !assigned.Any() || requested == null || !requested.Any()) return Enumerable.Empty<String>().ToList();
			HashSet<String> assignedSet = assigned.Select(x => x.ToLower()).ToHashSet();
			List<String> matched = requested.Select(x => x.ToLower()).Where(x => assignedSet.Contains(x)).ToList();
			return matched;
		}
	}
}
