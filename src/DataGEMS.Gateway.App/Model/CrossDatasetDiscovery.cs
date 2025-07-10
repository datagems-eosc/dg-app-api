
namespace DataGEMS.Gateway.App.Model
{
	public class CrossDatasetDiscovery
	{
		//GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public String Content { get; set; }
		public Dataset Dataset { get; set; }
		public String ObjectId { get; set; }
		public Decimal Distance { get; set; }
	}
}
