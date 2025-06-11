using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Accounting
{
	public enum KnownActions : short
	{
		None = 0,
		Foo = 1,
		Bar = 2,
	}

	public enum KnownUnits : short
	{
		Time = 0,
		Information = 1,
		Throughput = 2,
		Unit = 3,
	}

	public enum KnownTypes : short
	{
		Additive = 0,
		Subtractive = 1,
		Reset = 2,
	}

	public class AccountingInfo
	{
		public DateTime? TimeStamp { get; set; }
		public String ServiceId { get; set; }
		public KnownActions? Action {  get; set; }
		public String Resource { get; set; }
		public String UserId { get; set; }
		public String UserDelegate { get; set; }
		public String Value { get; set; }
		public KnownUnits? Measure { get; set; }
		public KnownTypes? Type { get; set; }
		public String Comment { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}
