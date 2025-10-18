//using DataGEMS.Gateway.App.Common.Auth;
//using System.Security.Claims;

//namespace Cite.Tools.Auth.Claims
//{
//	public static class Extensions
//	{
//		public static List<String> Datasets(this ClaimExtractor extractor, ClaimsPrincipal principal, String key = "Datasets")
//		{
//			return extractor.AsStrings(principal, key);
//		}

//		public static List<DatasetGrant> DatasetGrants(this ClaimExtractor extractor, ClaimsPrincipal principal, String key = "Datasets")
//		{
//			List<String> claims = extractor.Datasets(principal, key);
//			List<DatasetGrant> grants = claims.Select(x => x.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Where(x => x.Length == 4 && x.All(y => !String.IsNullOrEmpty(y))).Select(x =>
//			{
//				if (!String.Equals(x[0], "dataset", StringComparison.OrdinalIgnoreCase)) return null;
//				DatasetGrant.TargetType type;
//				switch (x[1])
//				{
//					case "group": { type = DatasetGrant.TargetType.Group; break; }
//					case "direct": { type = DatasetGrant.TargetType.Dataset; break; }
//					default: return null;
//				}

//				return new DatasetGrant()
//				{
//					Type = type,
//					Code = x[2],
//					Access = x[3]
//				};
//			}).Where(x => x != null).ToList();
//			return grants;
//		}
//	}
//}
