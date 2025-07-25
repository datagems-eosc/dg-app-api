
namespace DataGEMS.Gateway.App.Service.Conversation
{
	public class ConversationConfig
	{
		public ConversationTopicKind TopicKind { get; set; }

		public enum ConversationTopicKind : short
		{
			Date = 0,
			CurrentQuery = 1
		}
	}
}
