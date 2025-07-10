
namespace DataGEMS.Gateway.App.Exception
{
	public class DGForbiddenException : System.Exception
	{
		public int Code { get; set; }

		public DGForbiddenException() : base() { }
		public DGForbiddenException(int code) : this() { this.Code = code; }
		public DGForbiddenException(String message) : base(message) { }
		public DGForbiddenException(int code, String message) : this(message) { this.Code = code; }
		public DGForbiddenException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGForbiddenException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
