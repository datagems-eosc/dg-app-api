
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataSimpleExploreResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataExploreResponse; } }
		public DataGEMS.Gateway.App.Model.InDataExplore Payload { get; set; }
	}
}
