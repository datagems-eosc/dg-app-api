
namespace DataGEMS.Gateway.App.Formatting
{
	public interface IFormattingService
	{
		Task<string> FormatDateTime(DateTime? value, Guid? userId, string format = null, IFormatProvider provider = null);
		Task<string> FormatDateTime(DateTime? value, string format = null, IFormatProvider provider = null);
		Task<string> FormatDateOnly(DateOnly? value, Guid? userId, string format = null, IFormatProvider provider = null);
		Task<string> FormatDateOnly(DateOnly? value, string format = null, IFormatProvider provider = null);
		Task<string> FormatTimeOnly(TimeOnly? value, Guid? userId, string format = null, IFormatProvider provider = null);
		Task<string> FormatTimeOnly(TimeOnly? value, string format = null, IFormatProvider provider = null);
		Task<string> FormatInteger(int? value, Guid? userId, string format = null, IFormatProvider provider = null);
		Task<string> FormatInteger(int? value, string format = null, IFormatProvider provider = null);
		Task<string> FormatDecimal(decimal? value, Guid? userId, string format = null, IFormatProvider provider = null, int? decimals = null, MidpointRounding? midpointRounding = null);
		Task<string> FormatDecimal(decimal? value, string format = null, IFormatProvider provider = null, int? decimals = null, MidpointRounding? midpointRounding = null);
		Task<string> FormatPercentage(decimal? value, Guid? userId, string format = null, IFormatProvider provider = null);
		Task<string> FormatPercentage(decimal? value, string format = null, IFormatProvider provider = null);
		Task<string> FormatCurrency(decimal? value, Guid? userId, string symbol = null, string format = null, IFormatProvider provider = null);
		Task<string> FormatCurrency(decimal? value, string symbol = null, string format = null, IFormatProvider provider = null);
		Task<string> FormatTotalHours(decimal? value, Guid? userId, string format = null, IFormatProvider provider = null);
		Task<string> FormatTotalHours(decimal? value, string format = null, IFormatProvider provider = null);
	}
}
