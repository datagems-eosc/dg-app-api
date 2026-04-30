namespace DataGEMS.Gateway.App.Event
{
	public class OnAdHocQueryResultEventArgs
	{
		public OnAdHocQueryResultEventArgs(IEnumerable<Guid> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<Guid> Ids { get; private set; }

	}
}
