using Cite.Tools.Auth.Claims;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.LogTracking;
using Serilog.Context;
using System.Security.Claims;

namespace DataGEMS.Gateway.Api.LogTracking
{
	public class LogTrackingPrincipalMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly LogTrackingPrincipalConfig _config;

		public LogTrackingPrincipalMiddleware(RequestDelegate next, LogTrackingPrincipalConfig config)
		{
			this._next = next;
			this._config = config;
		}

		public async Task Invoke(
			HttpContext context, 
			ICurrentPrincipalResolverService currentPrincipalResolverService, 
			ClaimExtractor extractor)
		{
			ClaimsPrincipal principal = currentPrincipalResolverService.CurrentPrincipal();
			String subject = extractor.SubjectString(principal);
			String username = extractor.PreferredUsername(principal);
			String client = extractor.Client(principal);

			using (LogContext.PushProperty(this._config.SubjectAs, this._config.LogSubject ? subject : null))
			using (LogContext.PushProperty(this._config.UsernameAs, this._config.LogUsername ? username : null))
			using (LogContext.PushProperty(this._config.ClientAs, this._config.LogClient ? client : null))
			{
				await _next(context);
			}
		}
	}
}
