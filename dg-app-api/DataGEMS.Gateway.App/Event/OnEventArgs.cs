
namespace DataGEMS.Gateway.App.Event
{
	public class OnEventArgs
	{
		public OnEventArgs(IEnumerable<Guid> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<Guid> Ids { get; private set; }
	}
}
