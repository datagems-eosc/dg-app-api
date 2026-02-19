namespace DataGEMS.Gateway.App.Exception
{
	public class DGFoundManyException : System.Exception
	{
		public int Code { get; set; }

		public DGFoundManyException() : base() { }
		public DGFoundManyException(int code) : this() { this.Code = code; }
		public DGFoundManyException(String message) : base(message) { }
		public DGFoundManyException(int code, String message) : this(message) { this.Code = code; }
		public DGFoundManyException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGFoundManyException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
