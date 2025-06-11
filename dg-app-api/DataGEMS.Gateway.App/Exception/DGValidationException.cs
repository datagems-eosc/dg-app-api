using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Exception
{
	public class DGValidationException : System.Exception
	{
		public List<KeyValuePair<String, List<String>>> Errors { get; set; }
		public int Code { get; set; }

		public DGValidationException() : base("Validation Error") { }
		public DGValidationException(int code) : this() { this.Code = code; }
		public DGValidationException(String key, String error) : this() { this.Errors = new List<KeyValuePair<string, List<string>>>() { new KeyValuePair<string, List<string>>(key, new List<string>() { error }) }; }
		public DGValidationException(int code, String key, String error) : this(key, error) { this.Code = code; }
		public DGValidationException(KeyValuePair<String, List<String>> errors) : this() { this.Errors = new List<KeyValuePair<string, List<string>>>() { errors }; }
		public DGValidationException(int code, KeyValuePair<String, List<String>> errors) : this(errors) { this.Code = code; }
		public DGValidationException(List<KeyValuePair<String, List<String>>> errors) : this() { this.Errors = errors; }
		public DGValidationException(int code, List<KeyValuePair<String, List<String>>> errors) : this(errors) { this.Code = code; }
		public DGValidationException(String message) : base(message) { }
		public DGValidationException(int code, String message) : this(message) { this.Code = code; }
		public DGValidationException(String message, List<KeyValuePair<String, List<String>>> errors) : this(message) { this.Errors = errors; }
		public DGValidationException(int code, String message, List<KeyValuePair<String, List<String>>> errors) : this(message, errors) { this.Code = code; }
		public DGValidationException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGValidationException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
		public DGValidationException(String message, System.Exception innerException, List<KeyValuePair<String, List<String>>> errors) : this(message, innerException) { this.Errors = errors; }
		public DGValidationException(int code, String message, System.Exception innerException, List<KeyValuePair<String, List<String>>> errors) : this(message, innerException, errors) { this.Code = code; }
	}
}
