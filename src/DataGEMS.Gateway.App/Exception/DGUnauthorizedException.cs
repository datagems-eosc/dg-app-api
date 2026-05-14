
namespace DataGEMS.Gateway.App.Exception
{
	public class DGUnauthorizedException : System.Exception
	{
		public int Code { get; set; }

		public DGUnauthorizedException() : base() { }
		public DGUnauthorizedException(int code) : this() { this.Code = code; }
		public DGUnauthorizedException(String message) : base(message) { }
		public DGUnauthorizedException(int code, String message) : this(message) { this.Code = code; }
		public DGUnauthorizedException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGUnauthorizedException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
