using System.Text.RegularExpressions;

namespace DataGEMS.Gateway.App.Common.Validation
{
	public static class Extensions
	{
		public static Boolean IsValidEmail(this String value)
		{
			if (String.IsNullOrEmpty(value)) return false;
			try
			{
				new System.Net.Mail.MailAddress(value);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		public static Boolean IsValidE164Phone(this String value)
		{
			if (String.IsNullOrEmpty(value)) return false;
			try
			{
				return Regex.IsMatch(value, "^\\+?[1-9]\\d{1,14}$");
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		public static bool IsValidHttp(this string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return false;
			return Uri.TryCreate(value, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}

		public static bool IsValidFtp(this string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return false;
			return Uri.TryCreate(value, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeFtp || uriResult.Scheme == Uri.UriSchemeFtps);
		}

		public static bool IsValidPath(this string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return false;
			return !value.Any(c => Path.GetInvalidPathChars().Contains(c));
		}
	}
}
