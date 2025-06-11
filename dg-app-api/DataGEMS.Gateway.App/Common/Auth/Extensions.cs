using Cite.Tools.Auth;
using Cite.Tools.Auth.Claims;
using Cite.Tools.Auth.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cite.Tools.Auth.Claims
{
	public static class Extensions
	{
		public static List<String> Datasets(this ClaimExtractor extractor, ClaimsPrincipal principal, String key = "Datasets")
		{
			return extractor.AsStrings(principal, key);
		}
	}
}
