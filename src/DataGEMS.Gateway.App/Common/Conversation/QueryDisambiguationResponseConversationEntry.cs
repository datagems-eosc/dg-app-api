using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class QueryDisambiguationResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.QueryDisambiguationResponse; } }
		public QueryDisambiguationViewModel Payload { get; set; }
	}
}
