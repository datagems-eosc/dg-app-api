
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Exception
{
	public class DGUnderpinningException : System.Exception
	{
		public int Code { get; set; }

		public int? ErrorStatusCode { get; set; }
		public UnderpinningServiceType? ErrorSource {  get; set; }
		public String CorrelationId { get; set; }
		public String ResponsePayload { get; set; }

		public DGUnderpinningException() : base() { }
		public DGUnderpinningException(int code) : this() { this.Code = code; }
		public DGUnderpinningException(String message) : base(message) { }
		public DGUnderpinningException(int code, String message) : this(message) { this.Code = code; }
		public DGUnderpinningException(int code, String message, int? errorStatusCode = null, UnderpinningServiceType? errorSource = null, String correlationId = null, String responsePayload = null) : this(code, message) 
		{ 
			this.ErrorStatusCode = errorStatusCode;
			this.ErrorSource = errorSource;
			this.CorrelationId = correlationId;
			this.ResponsePayload = responsePayload;
		}
		public DGUnderpinningException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGUnderpinningException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
