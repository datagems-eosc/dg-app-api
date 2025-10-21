
namespace DataGEMS.Gateway.App.Event
{
	public class OnUserEventArgs
	{
		public OnUserEventArgs(IEnumerable<UserIdentifier> ids)
		{
			this.Ids = ids;
		}

		public IEnumerable<UserIdentifier> Ids { get; private set; }

		public class UserIdentifier
		{
			public Guid UserId { get; set; }
			public String SubjectId { get; set; }
		}
	}
}
