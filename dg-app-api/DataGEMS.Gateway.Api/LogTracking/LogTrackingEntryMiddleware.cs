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
				String requestScheme = invokerContextResolverService.RequestScheme();
				String cerSub = invokerContextResolverService.ClientCertificateSubjectName();
				String cerThumbprint = invokerContextResolverService.ClientCertificateThumbprint();
				if (this._config.Invoker?.IPAddress ?? false && ipAddress != null) entry.And("ip", ipAddress?.ToString());
				if (this._config.Invoker?.IPAddressFamily ?? false && ipAddress != null) entry.And("ip-family", ipAddress?.AddressFamily.ToString());
				if (this._config.Invoker?.RequestScheme ?? false && !String.IsNullOrEmpty(requestScheme)) entry.And("scheme", requestScheme);
				if (this._config.Invoker?.ClientCertificateSubjectName ?? false && !String.IsNullOrEmpty(cerSub)) entry.And("cer-sub", cerSub);
				if (this._config.Invoker?.ClientCertificateThumbpint ?? false && !String.IsNullOrEmpty(cerThumbprint)) entry.And("cer-thumbprint", cerThumbprint);

				ClaimsPrincipal principal = currentPrincipalResolverService.CurrentPrincipal();
				String subject = extractor.SubjectString(principal);
				String username = extractor.PreferredUsername(principal);
				String client = extractor.Client(principal);
				if (this._config.Principal?.Subject ?? false && !String.IsNullOrEmpty(subject)) entry.And("sub", subject);
				if (this._config.Principal?.Username ?? false && !String.IsNullOrEmpty(username)) entry.And("n", username);
				if (this._config.Principal?.Subject ?? false && !String.IsNullOrEmpty(client)) entry.And("c", client);

				logger.LogSafe(this._config.Level, entry);
			}

			await _next(context);
		}
	}
}
