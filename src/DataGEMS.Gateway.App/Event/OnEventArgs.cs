
namespace DataGEMS.Gateway.App.Event
{
	public class OnEventArgs<T>
	{
		public OnEventArgs(IEnumerable<T> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<T> Ids { get; private set; }
	}
}
