
namespace DataGEMS.Gateway.App.Exception
{
	public class DGUnderpinningException : System.Exception
	{
		public int Code { get; set; }

		public DGUnderpinningException() : base() { }
		public DGUnderpinningException(int code) : this() { this.Code = code; }
		public DGUnderpinningException(String message) : base(message) { }
		public DGUnderpinningException(int code, String message) : this(message) { this.Code = code; }
		public DGUnderpinningException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGUnderpinningException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
