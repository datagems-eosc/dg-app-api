namespace DataGEMS.Gateway.App.Event
{
	public class OnWorkflowProcessStepEventArgs
	{
		public OnWorkflowProcessStepEventArgs(IEnumerable<Guid> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<Guid> Ids { get; private set; }

	}
}
