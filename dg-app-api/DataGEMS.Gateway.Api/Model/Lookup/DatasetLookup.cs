namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class DatasetLookup : Lookup
	{
		public List<Guid> Ids { get; set; }
		public List<Guid> ExcludedIds { get; set; }
		public List<Guid> CollectionIds { get; set; }
		public String Like { get; set; }
	}
}
