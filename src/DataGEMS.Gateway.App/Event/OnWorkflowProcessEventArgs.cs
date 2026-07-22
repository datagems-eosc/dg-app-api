namespace DataGEMS.Gateway.App.Event
{
	public class OnWorkflowProcessEventArgs
	{
		public OnWorkflowProcessEventArgs(IEnumerable<Guid> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<Guid> Ids { get; private set; }

	}
}
