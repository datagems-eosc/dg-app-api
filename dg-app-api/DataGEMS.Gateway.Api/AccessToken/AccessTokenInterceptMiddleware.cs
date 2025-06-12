using DataGEMS.Gateway.App.AccessToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.Api.AccessToken
{
	public class AccessTokenInterceptMiddleware
	{
		private readonly RequestDelegate _next;

		public AccessTokenInterceptMiddleware(RequestDelegate next)
		{
			this._next = next;
		}

		public async Task Invoke(HttpContext context, ILogger<AccessTokenInterceptMiddleware> logger, RequestTokenIntercepted intercepted)
		{
			String accessToken = null;
			if (context.Request.Headers.ContainsKey(HeaderNames.Authorization))
			{
				if (!context.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues accessTokenHeaderValues) || accessTokenHeaderValues.Count != 1)
				{
					logger.LogError("Error extracting access token headers");
				}
				if (String.IsNullOrEmpty(accessTokenHeaderValues[0]))
				{
					logger.LogError("Error parsing access token headers");
				}
				accessToken = accessTokenHeaderValues[0].Replace(JwtBearerDefaults.AuthenticationScheme, "").Trim();
			}

			intercepted.AccessToken = accessToken;

			await _next(context);
		}

	}
}
