using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetPackaging.Model
{
	public class DatasetPackagingRecommendationResponse
	{
		[JsonProperty("packages")]
		public List<DatasetPackagingRecommendationResponse.Package> Packages { get; set; }

		public class Package
		{
			[JsonProperty("datasets")]
			public List<Guid> DatasetIds { get; set; }
			[JsonProperty("name")]
			public string Name { get; set; }
		}
	}
}
