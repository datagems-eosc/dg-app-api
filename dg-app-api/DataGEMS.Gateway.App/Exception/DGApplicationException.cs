using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Exception
{
	public class DGApplicationException : System.Exception
	{
		public int Code { get; set; }

		public DGApplicationException() : base() { }
		public DGApplicationException(int code) : this() { this.Code = code; }
		public DGApplicationException(String message) : base(message) { }
		public DGApplicationException(int code, String message) : this(message) { this.Code = code; }
		public DGApplicationException(String message, System.Exception innerException) : base(message, innerException) { }
		public DGApplicationException(int code, String message, System.Exception innerException) : this(message, innerException) { this.Code = code; }
	}
}
