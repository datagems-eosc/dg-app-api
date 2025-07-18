
namespace DataGEMS.Gateway.App.Model
{
	public class CrossDatasetDiscovery
	{
		//GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";
		public Dataset Dataset { get; set; }
		public Decimal? MaxSimilarity { get; set; }
		public List<DatasetHits> Hits { get; set; }

		public class DatasetHits
		{
			public String Content { get; set; }
			public String ObjectId { get; set; }
			public Decimal? Similarity { get; set; }
		}
	}
}
