
namespace DataGEMS.Gateway.App.Common.Auth
{
	public class DatasetGrant
	{
		public TargetType Type { get; set; }
		public String Code { get; set; }
		public String Access {  get; set; }

		public enum TargetType : short
		{
			Dataset = 0,
			Group = 1
		}
	}
}
