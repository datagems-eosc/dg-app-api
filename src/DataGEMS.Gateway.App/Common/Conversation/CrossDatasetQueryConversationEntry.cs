
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class CrossDatasetQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.CrossDatasetQuery; } }
		public Service.Discovery.DiscoverInfo Payload { get; set; }
	}
}
