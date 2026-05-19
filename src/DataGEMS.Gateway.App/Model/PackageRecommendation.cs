namespace DataGEMS.Gateway.App.Model
{
	public class PackageRecommendation
	{
		public List<Package> Packages { get; set; }

		public class Package
		{
			public string Name { get; set; }
			public List<Dataset> Datasets { get; set; }
		}
	}

	public class PackageRecommendationRequest
	{
		public List<Guid> DatasetIds { get; set; }
		public int PackagesCount { get; set; }
		public int DatasetsPerPackage { get; set; }
	}
}
