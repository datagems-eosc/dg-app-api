
using System.ComponentModel;

namespace DataGEMS.Gateway.App.Common.Auth
{
	public class ContextGrant
	{
		public String PrincipalId { get; set; }
		public PrincipalKind PrincipalType { get; set; }
		public TargetKind TargetType { get; set; }
		public Guid TargetId { get; set; }
		public String Role { get; set; }

		public enum PrincipalKind : short
		{
			[Description("Grant assigned at the user level")]
			User = 0,
			[Description("Grant assigned at the user group level")]
			Group = 1
		}

		public enum TargetKind : short
		{
			[Description("Grant assigned at the dataset level")]
			Dataset = 0,
			[Description("Grant assigned at the collection level")]
			Collection = 1
		}
	}
}
