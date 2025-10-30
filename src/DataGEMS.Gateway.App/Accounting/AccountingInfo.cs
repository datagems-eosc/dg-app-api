
namespace DataGEMS.Gateway.App.Accounting
{
	public enum KnownActions : short
	{
		None = 0,
		Query = 1,
		Persist = 2,
		Delete = 3,
		Invoke = 4,
		Onboard = 5,
		Profile = 6,
		Upload = 7,
	}

	public enum KnownUnits : short
	{
		Time = 0,
		Information = 1,
		Throughput = 2,
		Unit = 3,
	}

	public enum KnownTypes : short
	{
		Additive = 0,
		Subtractive = 1,
		Reset = 2,
	}

	public enum KnownResources : short
	{
		Dataset = 0,
		Collection = 1,
		User = 2,
		UserCollection = 3,
		Conversation = 4,
		CrossDatasetDiscovery = 5,
		Vocabulary = 6,
		InDataExploration = 7,
		Workflow = 8,
		UserSettings = 9,
		ContextGrant = 10,
		ContextGrantAssignment = 11,
		UserGroup = 12,
	}

	public class AccountingInfo
	{
		public DateTime? TimeStamp { get; set; }
		public String ServiceId { get; set; }
		public KnownActions? Action {  get; set; }
		public String Resource { get; set; }
		public String UserId { get; set; }
		public String UserDelegate { get; set; }
		public String Value { get; set; }
		public KnownUnits? Measure { get; set; }
		public KnownTypes? Type { get; set; }
		public String Comment { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}
