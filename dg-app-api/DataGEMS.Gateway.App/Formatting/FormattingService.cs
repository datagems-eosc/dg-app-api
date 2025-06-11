using Cite.Tools.Auth.Claims;
using Cite.WebTools.CurrentPrincipal;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DataGEMS.Gateway.App.Formatting
{
	public class FormattingService : IFormattingService
	{
		private readonly FormattingServiceConfig _config;
		private readonly FormattingCache _formattingCache;
		private readonly ILogger<FormattingService> _logger;
		private readonly ICurrentPrincipalResolverService _principalResolverService;
		private readonly ClaimExtractor _extractor;

		public FormattingService(
			FormattingServiceConfig config,
			FormattingCache formattingCache,
			ILogger<FormattingService> logger,
			ICurrentPrincipalResolverService principalResolverService,
			ClaimExtractor extractor)
		{
			_config = config;
			_formattingCache = formattingCache;
			_logger = logger;
			_principalResolverService = principalResolverService;
			_extractor = extractor;
		}

		private static string Format(DateTime? value, string format = null, IFormatProvider provider = null)
		{
			if (!value.HasValue) return string.Empty;

			if (string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(provider);
			else if (!string.IsNullOrEmpty(format) && provider == null) return value.Value.ToString(format);
			else if (!string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(format, provider);

			return value.Value.ToString();
		}

		private static string Format(DateOnly? value, string format = null, IFormatProvider provider = null)
		{
			if (!value.HasValue) return string.Empty;

			if (string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(provider);
			else if (!string.IsNullOrEmpty(format) && provider == null) return value.Value.ToString(format);
			else if (!string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(format, provider);

			return value.Value.ToString();
		}

		private static string Format(TimeOnly? value, string format = null, IFormatProvider provider = null)
		{
			if (!value.HasValue) return string.Empty;

			if (string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(provider);
			else if (!string.IsNullOrEmpty(format) && provider == null) return value.Value.ToString(format);
			else if (!string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(format, provider);

			return value.Value.ToString();
		}

		private static string Format(int? value, string format = null, IFormatProvider provider = null)
		{
			if (!value.HasValue) return string.Empty;

			if (string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(provider);
			else if (!string.IsNullOrEmpty(format) && provider == null) return value.Value.ToString(format);
			else if (!string.IsNullOrEmpty(format) && provider != null) return value.Value.ToString(format, provider);

			return value.Value.ToString();
		}

		private static string Format(decimal? value, string format = null, IFormatProvider provider = null, int? decimals = null, MidpointRounding? midpointRounding = null)
		{
			if (!value.HasValue) return string.Empty;

			decimal val = value.Value;

			if (decimals.HasValue)
			{
				if (midpointRounding.HasValue) val = Math.Round(val, decimals.Value, midpointRounding.Value);
				else val = Math.Round(val, decimals.Value);
			}

			if (string.IsNullOrEmpty(format) && provider != null) return val.ToString(provider);
			else if (!string.IsNullOrEmpty(format) && provider == null) return val.ToString(format);
			else if (!string.IsNullOrEmpty(format) && provider != null) return val.ToString(format, provider);

			return val.ToString();
		}

		public async Task<string> FormatDateTime(DateTime? value, Guid? userId, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Format)) formatToUse = _config.Override.IntegerFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.IntegerFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.IntegerFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.IntegerFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "dateTime culture was not found");
			}

			return Format(value, formatToUse, providerToUse);
		}

		public async Task<string> FormatDateTime(DateTime? value, string format = null, IFormatProvider provider = null)
		{
			return await FormatDateTime(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		public async Task<string> FormatDateOnly(DateOnly? value, Guid? userId, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Format)) formatToUse = _config.Override.IntegerFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.IntegerFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.IntegerFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.IntegerFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "dateOnly culture was not found");
			}

			return Format(value, formatToUse, providerToUse);
		}

		public async Task<string> FormatDateOnly(DateOnly? value, string format = null, IFormatProvider provider = null)
		{
			return await FormatDateOnly(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		public async Task<string> FormatTimeOnly(TimeOnly? value, Guid? userId, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Format)) formatToUse = _config.Override.IntegerFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.IntegerFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.IntegerFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.IntegerFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "timeOnly culture was not found");
			}

			return Format(value, formatToUse, providerToUse);
		}

		public async Task<string> FormatTimeOnly(TimeOnly? value, string format = null, IFormatProvider provider = null)
		{
			return await FormatTimeOnly(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		public async Task<string> FormatInteger(int? value, Guid? userId, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Format)) formatToUse = _config.Override.IntegerFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.IntegerFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.IntegerFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.IntegerFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.IntegerFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "integer culture was not found");
			}

			return Format(value, formatToUse, providerToUse);
		}

		public async Task<string> FormatInteger(int? value, string format = null, IFormatProvider provider = null)
		{
			return await FormatInteger(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		public async Task<string> FormatDecimal(decimal? value, Guid? userId, string format = null, IFormatProvider provider = null, int? decimals = null, MidpointRounding? midpointRounding = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.DecimalFormat.Format)) formatToUse = _config.Override.DecimalFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.DecimalFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.DecimalFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.DecimalFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.DecimalFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.DecimalFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "decimal culture was not found");
			}

			return Format(value, formatToUse, providerToUse, decimals, midpointRounding);
		}

		public async Task<string> FormatDecimal(decimal? value, string format = null, IFormatProvider provider = null, int? decimals = null, MidpointRounding? midpointRounding = null)
		{
			return await FormatDecimal(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		public async Task<string> FormatPercentage(decimal? value, Guid? userId, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.PercentageFormat.Format)) formatToUse = _config.Override.PercentageFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.PercentageFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.PercentageFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.PercentageFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.PercentageFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.PercentageFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "percentage culture was not found");
			}

			return Format(value, format: formatToUse, provider: providerToUse);
		}

		public async Task<string> FormatPercentage(decimal? value, string format = null, IFormatProvider provider = null)
		{
			return await FormatPercentage(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		public async Task<string> FormatCurrency(decimal? value, Guid? userId, string symbol = null, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.CurrencyFormat.Format)) formatToUse = _config.Override.CurrencyFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.CurrencyFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.CurrencyFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.CurrencyFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.CurrencyFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.CurrencyFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "currency culture was not found");
			}

			string formattedDecimal = Format(value, format: formatToUse, provider: providerToUse);

			return $"{formattedDecimal}{symbol}";
		}

		public async Task<string> FormatCurrency(decimal? value, string symbol = null, string format = null, IFormatProvider provider = null)
		{
			return await FormatCurrency(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), symbol, format, provider);
		}

		public async Task<string> FormatTotalHours(decimal? value, Guid? userId, string format = null, IFormatProvider provider = null)
		{
			string formatToUse;
			if (!string.IsNullOrEmpty(_config.Override.TotalHoursFormat.Format)) formatToUse = _config.Override.TotalHoursFormat.Format;
			else if (!string.IsNullOrEmpty(format)) formatToUse = format;
			else formatToUse = _config.Default.TotalHoursFormat.Format;

			IFormatProvider providerToUse = null;
			try
			{
				if (!string.IsNullOrEmpty(_config.Override.TotalHoursFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Override.TotalHoursFormat.Culture);
				else if (provider != null) providerToUse = provider;
				else providerToUse = await GetFormatProvider(userId);

				if (providerToUse == null && !string.IsNullOrEmpty(_config.Default.TotalHoursFormat.Culture)) providerToUse = CultureInfo.GetCultureInfo(_config.Default.TotalHoursFormat.Culture);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogError(ex, "currency culture was not found");
			}

			string formattedDecimal = Format(value, format: formatToUse, provider: providerToUse);

			return formattedDecimal;
		}

		public async Task<string> FormatTotalHours(decimal? value, string format = null, IFormatProvider provider = null)
		{
			return await FormatTotalHours(value, _extractor.SubjectGuid(_principalResolverService.CurrentPrincipal()), format, provider);
		}

		private async Task<IFormatProvider> GetFormatProvider(Guid? userId)
		{
			if (!userId.HasValue) return null;

			UserFormattingProfile profile = await _formattingCache.CacheLookup(userId.Value);
			if (profile == null)
			{
				//TODO: Add some logic o retrieve user profile if and when becomes available
				//profile = new UserFormattingProfile
				//{
				//	Culture = data.Culture,
				//	Timezone = data.Timezone,
				//	Language = data.Language,
				//};
				//await _formattingCache.CacheUpdate(userId.Value, profile);
				profile = null;
			}
			if (profile == null) return null;

			try { return CultureInfo.GetCultureInfo(profile.Culture); }
			catch { return null; }
		}
	}
}
