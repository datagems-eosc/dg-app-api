using Cite.Tools.Common.Extensions;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Time;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;

namespace DataGEMS.Gateway.App.Common
{
	public static class Extensions
	{
		public static List<String> ParseCsv(this String value)
		{
			if (String.IsNullOrEmpty(value)) return null;
			String[] values = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			return values.ToList();
		}

		public static String ToETag(this DateTime updatedAt)
		{
			return updatedAt.ToEpoch().ToString();
		}

		public static String ToSha256(this String input)
		{
			if (String.IsNullOrEmpty(input)) return String.Empty;
			using (var sha = SHA256.Create())
			{
				var bytes = Encoding.UTF8.GetBytes(input);
				var hash = sha.ComputeHash(bytes);

				return Convert.ToBase64String(hash);
			}
		}

		public static Boolean IsNotNullButEmpty<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable == null) return false;
			return !enumerable.Any();
		}

		public static IFieldSet ExtractNonPrefixed(this IFieldSet fieldSet, String qualifier = ".")
		{
			if (fieldSet == null) return null;

			List<String> nonQualified = fieldSet.Fields.Where(x => !x.Contains(qualifier)).ToList();
			return new FieldSet(nonQualified);
		}

		public static IFieldSet MergeAsPrefixed(this IFieldSet fieldSet, IFieldSet other, String prefix, String qualifier = ".")
		{
			if (other == null || other.IsEmpty()) return fieldSet;

			List<String> qualifiedOthers = other.Fields.Select(x => new String[] { prefix, x }.AsIndexer(qualifier)).ToList();
			IFieldSet merged = fieldSet.Merge(new FieldSet(qualifiedOthers));
			return merged;
		}

		public static bool CensoredAsUnauthorized(this IFieldSet requested, IFieldSet censored, String qualifier = ".")
		{
			Boolean isRequestedNonEmpty = requested != null && !requested.IsEmpty();
			Boolean isCensoredEmpty = censored == null || censored.IsEmpty();

			Boolean hasRequestedTopLevel = requested != null && !requested.IsEmpty() && requested.Fields.Any(x => !x.Contains(qualifier));
			Boolean hasCensoredTopLevel = censored != null && !censored.IsEmpty() && censored.Fields.Any(x => !x.Contains(qualifier));

			Boolean isTopLevelFiltered = hasRequestedTopLevel && !hasCensoredTopLevel;

			return (isRequestedNonEmpty && isCensoredEmpty) || isTopLevelFiltered;
		}

		public static List<String> ReduceToAssignedPermissions(this IEnumerable<String> requested, IEnumerable<String> assigned)
		{
			if (assigned == null || !assigned.Any() || requested == null || !requested.Any()) return Enumerable.Empty<String>().ToList();
			HashSet<String> assignedSet = assigned.Select(x => x.ToLower()).ToHashSet();
			List<String> matched = requested.Select(x => x.ToLower()).Where(x => assignedSet.Contains(x)).ToList();
			return matched;
		}


		public static string TransformJTokenToString(JObject obj, string propName)
		{
			if (!obj.TryGetValue(propName, out var token) || token is null || token.Type is JTokenType.Null or JTokenType.Undefined) return null;
			return token.Type switch
			{
				JTokenType.String => token.Value<string>(),
				JTokenType.Integer or JTokenType.Float or JTokenType.Boolean or JTokenType.Date => token.ToString(),
				_ => throw new InvalidCastException($"Token type {token.Type} cannot be converted to string.")
			};
		}

		public static Guid TransformJTokenToGuid(JObject obj, string propName)
		{
			if (!obj.TryGetValue(propName, out var token) || token is null || token.Type is JTokenType.Null or JTokenType.Undefined)
				return Guid.Empty;
			if (token.Type == JTokenType.Guid)
				return token.Value<Guid>();
			if (token.Type == JTokenType.String && Guid.TryParse(token.Value<string>(), out var guid))
				return guid;
			throw new InvalidCastException($"Token type {token.Type} cannot be converted to Guid.");
		}

		public static long? TransformJTokenToLong(JObject obj, string propName)
		{
			if (!obj.TryGetValue(propName, out var token) || token is null || token.Type is JTokenType.Null or JTokenType.Undefined) return null;
			return token.Type switch
			{
				JTokenType.Integer => token.Value<long?>(),
				JTokenType.Float => token.Value<long?>(),
				_ => throw new InvalidCastException($"Token type {token.Type} cannot be converted to long?.")
			};
		}

		public static DateOnly? TransformJTokenToDateOnly(JObject obj, string propName)
		{
			if (!obj.TryGetValue(propName, out var token) || token is null || token.Type is JTokenType.Null or JTokenType.Undefined)
				return null;
			var s = (token.Type == JTokenType.String ? token.Value<string>() : token.ToString())?.Trim();
			if (string.IsNullOrWhiteSpace(s)) return null;
			if (DateOnly.TryParseExact(
					s,
					["dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy"],
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var d))
			{
				return d;
			}
			if (DateTime.TryParse(
					s,
					CultureInfo.InvariantCulture,
					DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
					out var dt))
			{
				return DateOnly.FromDateTime(dt);
			}
			return null;
		}

		public static List<string> TransformJTokenToStringList(JObject obj, string propName)
		{
			if (!obj.TryGetValue(propName, out var token) || token is null ||
			token.Type is JTokenType.Null or JTokenType.Undefined)
			{
				return null;
			}
			if (token is JArray arr)
				return arr.Values<string>().Where(x => x is not null).Cast<string>().ToList();
			if (token.Type == JTokenType.String)
				return [token.Value<string>()];

			throw new InvalidCastException($"Token type {token.Type} cannot be converted to List<string>.");
		}

		public static List<JToken> JArrayToList(object obj)
		{
			if (obj is JArray jArray)
			{
				return jArray.ToList();
			}
			else if (obj is IEnumerable<JToken> enumerable)
			{
				return enumerable.ToList();
			}
			else
			{
				throw new InvalidCastException($"Object{obj} cannot be converted to List<string>.");
			}
		}

		public static async Task<string> SendGetRequest<TLogger>(
			this IHttpClientFactory clientFactory,
			ILogger<TLogger> logger,
			ErrorThesaurus errors,
			LogCorrelationScope logCorrelationScope,
			string url,
			Dictionary<string, string> headers = null,
			bool locationHeaderReturn = false)
		{
			logger.Debug("Sending GET request to {url}", url);
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
			if (headers != null)
			{
				foreach (var header in headers)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}
			HttpResponseMessage response = null;
			try { 
				response = await clientFactory.CreateClient().SendAsync(request);
				logger.Debug("Received response with status code {statusCode}", response?.StatusCode);
			}
			catch (System.Exception ex)
			{
				logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(errors.UnderpinningService.Code, errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, logCorrelationScope.CorrelationId);
			}

			try
			{
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
				else response.EnsureSuccessStatusCode();
			}
			catch (System.Exception ex)
			{
				string errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				bool includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(errors.UnderpinningService.Code, errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			string content = await response.Content.ReadAsStringAsync();
			logger.Debug("Response content: {content}", content);
			if (locationHeaderReturn) content = response.Headers.Location?.ToString();
			return content;
		}

		public static async Task<string> SendPostRequestWithRawBody<TLogger, TBody>(
			this IHttpClientFactory clientFactory,
			ILogger<TLogger> logger,
			ErrorThesaurus errors,
			LogCorrelationScope logCorrelationScope,
			JsonHandlingService jsonHandlingService,
			string url,
			TBody body,
			Dictionary<string, string> headers = null,
			bool locationHeaderReturn = false)
		{
			logger.Debug("Sending POST request to {url}", url);
			string requestContent = body != null ? jsonHandlingService.ToJson(body) : null;
			logger.Debug("Request content: {content}", requestContent);
			HttpRequestMessage request = body != null ? new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(requestContent, Encoding.UTF8, MediaTypeNames.Application.Json)
			} : new HttpRequestMessage(HttpMethod.Post, url);
			if (headers != null)
			{
				foreach (var header in headers)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}
			HttpResponseMessage response = null;
			try { 
				response = await clientFactory.CreateClient().SendAsync(request);
				logger.Debug("Received response with status code {statusCode}", response?.StatusCode);
			}
			catch (System.Exception ex)
			{
				logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(errors.UnderpinningService.Code, errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, logCorrelationScope.CorrelationId);
			}

			try
			{
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
				else response.EnsureSuccessStatusCode();
			}
			catch (System.Exception ex)
			{
				string errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				bool includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(errors.UnderpinningService.Code, errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.AAI, logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			string content = await response.Content.ReadAsStringAsync();
			logger.Debug("Response content: {content}", content);
			if (locationHeaderReturn) content = response.Headers.Location?.ToString();
			return content;
		}

		public static async Task<string> SendPostRequestFormUrlEncoded<TLogger>(
			this IHttpClientFactory clientFactory,
			ILogger<TLogger> logger,
			ErrorThesaurus errors,
			LogCorrelationScope logCorrelationScope,
			string url,
			Dictionary<string, string> formBody,
			Dictionary<string, string> headers = null,
			bool locationHeaderReturn = false)
		{
			logger.Debug("Sending POST form-urlencoded request to {url}", url);

			FormUrlEncodedContent formContent = new FormUrlEncodedContent(
				formBody ?? new Dictionary<string, string>()
			);

			string requestContent = await formContent.ReadAsStringAsync();
			logger.Debug("Request content: {content}", requestContent);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = formContent
			};

			if (headers != null)
			{
				foreach (var header in headers)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}

			HttpResponseMessage response = null;

			try
			{
				response = await clientFactory.CreateClient().SendAsync(request);
				logger.Debug("Received response with status code {statusCode}", response?.StatusCode);
			}
			catch (System.Exception ex)
			{
				logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");

				throw new DGUnderpinningException(
					errors.UnderpinningService.Code,
					errors.UnderpinningService.Message,
					(int?)response?.StatusCode,
					UnderpinningServiceType.AAI,
					logCorrelationScope.CorrelationId
				);
			}

			try
			{
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
				response.EnsureSuccessStatusCode();
			}
			catch (System.Exception ex)
			{
				string errorPayload = null;

				try { errorPayload = await response.Content.ReadAsStringAsync(); }
				catch (System.Exception) { }

				logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);

				bool includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;

				throw new Exception.DGUnderpinningException(
					errors.UnderpinningService.Code,
					errors.UnderpinningService.Message,
					(int?)response?.StatusCode,
					UnderpinningServiceType.AAI,
					logCorrelationScope.CorrelationId,
					includeErrorPayload ? errorPayload : null
				);
			}
			string content = await response.Content.ReadAsStringAsync();
			logger.Debug("Response content: {content}", content);
			if (locationHeaderReturn) content = response.Headers.Location?.ToString();

			return content;
		}
	}
}
