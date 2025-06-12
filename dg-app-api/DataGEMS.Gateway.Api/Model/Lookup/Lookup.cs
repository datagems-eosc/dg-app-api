using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Query;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class Lookup
	{
		public class Header
		{
			public bool CountAll { get; set; }
		}

		public Paging Page { get; set; }

		public Ordering Order { get; set; }

		public Header Metadata { get; set; }

		public FieldSet Project { get; set; }
	}
}
