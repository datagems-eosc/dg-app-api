namespace DataGEMS.Gateway.App.Service.DatasetRecommender
{
	public class DatasetRecommenderHttpConfig
	{
		public String Scope { get; set; }
		public String BaseUrl { get; set; }
		public String ExistEndpoint { get; set; }
		public string RecommendEndpoint { get; set; }
		public int DefaultRecommendationDatasets { get; set; }
	}
}
