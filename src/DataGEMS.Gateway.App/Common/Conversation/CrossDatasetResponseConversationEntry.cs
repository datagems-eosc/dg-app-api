
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class CrossDatasetResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.CrossDatasetResponse; } }
		public List<DataGEMS.Gateway.App.Model.CrossDatasetDiscovery> Payload { get; set; }
	}
}
