using DataGEMS.Gateway.App.LogTracking;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace DataGEMS.Gateway.Api.LogTracking
{
	public class LogTrackingCorrelationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly LogTrackingCorrelationConfig _config;
		public LogTrackingCorrelationMiddleware(RequestDelegate next, LogTrackingCorrelationConfig config)
		{
			this._next = next;
			this._config = config;
		}

		public async Task Invoke(HttpContext context, ILogger<LogTrackingCorrelationMiddleware> logger)
		{
			String trackingContext;
			if (context.Request.Headers.ContainsKey(this._config.HeaderName))
			{
				if (!context.Request.Headers.TryGetValue(this._config.HeaderName, out StringValues trackingContextHeaderValues) || trackingContextHeaderValues.Count != 1)
				{
					logger.LogError("Error extracting logtracking headers");
				}
				if(String.IsNullOrEmpty(trackingContextHeaderValues[0]))
				{
					logger.LogError("Error parsing logtracking headers");
				}
				trackingContext = trackingContextHeaderValues[0];
			}
			else trackingContext = Guid.NewGuid().ToString();

			using (LogContext.PushProperty(this._config.LogAs, trackingContext))
			{
				await _next(context);
			}
		}
	}
}
