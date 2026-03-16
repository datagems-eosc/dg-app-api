
namespace DataGEMS.Gateway.App.Event
{
	public class OnDatasetCollectionEventArgs
	{
		public OnDatasetCollectionEventArgs(IEnumerable<DatasetCollectionIdentifier> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<DatasetCollectionIdentifier> Ids { get; private set; }

		public class DatasetCollectionIdentifier
		{
			public Guid DatasetId { get; set; }
			public Guid CollectionId { get; set; }
		}
	}
}
