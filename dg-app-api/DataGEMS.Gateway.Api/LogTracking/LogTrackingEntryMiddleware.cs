using Cite.Tools.Auth.Claims;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using Cite.WebTools.InvokerContext;
using DataGEMS.Gateway.App.LogTracking;
using System.Net;
using System.Security.Claims;

namespace DataGEMS.Gateway.Api.LogTracking
{
	public class LogTrackingEntryMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly LogTrackingEntryConfig _config;

		public LogTrackingEntryMiddleware(RequestDelegate next, LogTrackingEntryConfig config)
		{
			this._next = next;
			this._config = config;
		}

		public async Task Invoke(
			HttpContext context, 
			ILogger<LogTrackingEntryMiddleware> logger,
			ICurrentPrincipalResolverService currentPrincipalResolverService, 
			IInvokerContextResolverService invokerContextResolverService, 
			ClaimExtractor extractor)
		{
			if (this._config.Enabled)
			{
				MapLogEntry entry = new MapLogEntry();

				IPAddress ipAddress = invokerContextResolverService.RemoteIpAddress();
				if (this._config.Invoker?.IPAddress ?? false) entry.And("ip", ipAddress?.ToString());
				if (this._config.Invoker?.IPAddressFamily ?? false) entry.And("ip-family", ipAddress?.AddressFamily.ToString());
				if (this._config.Invoker?.RequestScheme ?? false) entry.And("scheme", invokerContextResolverService.RequestScheme());
				if (this._config.Invoker?.ClientCertificateSubjectName ?? false) entry.And("cer-sub", invokerContextResolverService.ClientCertificateSubjectName());
				if (this._config.Invoker?.ClientCertificateThumbpint ?? false) entry.And("cer-thumbprint", invokerContextResolverService.ClientCertificateThumbprint());

				ClaimsPrincipal principal = currentPrincipalResolverService.CurrentPrincipal();
				if (this._config.Principal?.Subject ?? false) entry.And("sub", extractor.SubjectString(principal));
				if (this._config.Principal?.Username ?? false) entry.And("n", extractor.PreferredUsername(principal));
				if (this._config.Principal?.Subject ?? false) entry.And("c", extractor.Client(principal));

				logger.LogSafe(this._config.Level, entry);
			}

			await _next(context);
		}
	}
}
