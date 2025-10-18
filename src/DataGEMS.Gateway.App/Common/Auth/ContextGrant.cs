
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common.Auth
{
	public class ContextGrant
	{
		public String GroupId { get; set; }
		public TargetType Type { get; set; }
		public String Code { get; set; }
		public String Access {  get; set; }

		public enum TargetType : short
		{
			[Description("Grant assigned at the dataset level")]
			Dataset = 0,
			[Description("Grant assigned at the group level")]
			Group = 1
		}
	}
}
