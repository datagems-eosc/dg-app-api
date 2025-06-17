
namespace DataGEMS.Gateway.App.Exception
{
	public class DGNotFoundException : System.Exception
	{
		public int Code { get; set; }

		public DGNotFoundException() : base() { }
		public DGNotFoundException(int code) : this() { this.Code = code; }
		public DGNotFoundException(String message) : base(message) { }
		public DGNotFoundException(int code, String message) : this(message) { this.Code = code; }
		public DGNotFoundException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGNotFoundException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
