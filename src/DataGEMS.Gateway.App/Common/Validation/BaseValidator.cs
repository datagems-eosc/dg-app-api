using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DataGEMS.Gateway.App.Common.Validation
{
	public abstract class BaseValidator<T> : AbstractValidator<T>
	{
		protected ValidatorFactory _validatorFactory;
		protected ILogger _logger;
		protected ErrorThesaurus _errors;

		public BaseValidator(
			ValidatorFactory validatorFactory,
			ILogger logger,
			ErrorThesaurus errors)
		{
			this._validatorFactory = validatorFactory;
			this._logger = logger;
			this._errors = errors;
		}

		public override void ValidateForce(Object item)
		{
			if (this.Validate(item).Result.IsValid) return;
			List<KeyValuePair<string, List<string>>> errors = this.FlattenValidationResult().Select(x => new KeyValuePair<string, List<string>>(x.Key, x.Value)).ToList();
			this._logger.Debug(new DataLogEntry("validation failed", errors));
			throw new DGValidationException(this._errors.ModelValidation.Code, errors);
		}

		protected Boolean IsValidId(int? id)
		{
			return id.HasValue && id.Value > 0;
		}

		protected Boolean IsValidId(String id)
		{
			if (!int.TryParse(id, out int tmp)) return false;
			return this.IsValidId(tmp);
		}

		protected Boolean IsValidGuid(Guid? guid)
		{
			return guid.HasValue && guid.Value != Guid.Empty;
		}

		protected Boolean IsValidGuid(String id)
		{
			if (!Guid.TryParse(id, out Guid tmp)) return false;
			return this.IsValidGuid(tmp);
		}

		protected Boolean IsValidHash(String hash)
		{
			return !String.IsNullOrEmpty(hash);
		}

		protected Boolean IsEmpty(String value)
		{
			return String.IsNullOrEmpty(value);
		}

		protected Boolean IsNotEmpty(String value)
		{
			return !this.IsEmpty(value);
		}

		protected Boolean IsValidEmail(String value)
		{
			return value.IsValidEmail();
		}

		protected Boolean IsValidE164Phone(String value)
		{
			return value.IsValidE164Phone();
		}

		protected Boolean IsValidCulture(String culture)
		{
			try
			{
				CultureInfo.GetCultureInfo(culture);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		protected Boolean IsValidTimeZone(String timezone)
		{
			try
			{
				TimeZoneInfo.FindSystemTimeZoneById(timezone);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		protected Boolean HasValue<S>(Nullable<S> value) where S : struct
		{
			return value.HasValue;
		}

		protected Boolean LessEqual(String value, int size)
		{
			return value.Length <= size;
		}
	}
}
