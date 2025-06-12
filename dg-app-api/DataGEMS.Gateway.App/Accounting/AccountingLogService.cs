using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DataGEMS.Gateway.App.Accounting
{
	public class AccountingLogService : IAccountingService
	{
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly ILogger _logger;
		private readonly AccountingLogConfig _config;
		private readonly ClaimExtractor _extractor;

		public AccountingLogService(
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			ILoggerFactory logger,
			AccountingLogConfig config,
			ClaimExtractor extractor)
		{
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._config = config;
			this._extractor = extractor;
			this._logger = logger.CreateLogger("accounting");
		}

		public Boolean IsEnabled { get { return this._config.Enable; } }

		public void AccountFor(KnownActions action)
		{
			if (!this.IsEnabled) return;
			this.AccountFor(new AccountingInfo() { Action = action });
		}

		public void AccountFor(KnownActions action, String resource)
		{
			if (!this.IsEnabled) return;
			this.AccountFor(new AccountingInfo() { Action = action, Resource = resource });
		}

		public void AccountFor(AccountingInfo info)
		{
			if (!this.IsEnabled) return;
			MapLogEntry entry = new MapLogEntry()
				.And("timestamp", (info.TimeStamp ?? DateTime.UtcNow).ToString("O"))
				.And("serviceId", !String.IsNullOrEmpty(info.ServiceId) ? info.ServiceId : this._config.ServiceId)
				.And("action", (info.Action ?? KnownActions.None).ToString())
				.And("resource", !String.IsNullOrEmpty(info.Resource) ? info.Resource : "N/A")
				.And("userId", !String.IsNullOrEmpty(info.UserId) ? info.UserId : this._extractor.SubjectString(this._currentPrincipalResolverService.CurrentPrincipal()) ?? "N/A")
				.And("value", !String.IsNullOrEmpty(info.Value) ? info.Value : "1")
				.And("measure", (info.Measure ?? KnownUnits.Unit).ToString())
				.And("type", (info.Type ?? KnownTypes.Additive).AsArray().Select(x =>
				{
					switch (x)
					{
						case KnownTypes.Additive: return "+";
						case KnownTypes.Subtractive: return "-";
						case KnownTypes.Reset: return "0";
						default: return "+";
					};
				}).FirstOrDefault());

			if (!String.IsNullOrEmpty(info.UserDelegate)) entry.And("userDelegate", info.UserDelegate);
			if (!String.IsNullOrEmpty(info.Comment)) entry.And("comment", info.Comment);
			if (info.StartTime.HasValue) entry.And("startTime", info.StartTime.Value.ToString("O"));
			if (info.EndTime.HasValue) entry.And("endTime", info.EndTime.Value.ToString("O"));

			this._logger.LogSafe(this._config.Level, entry);
		}
	}
}
