using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class IntentDisambiguationResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.IntentDisambiguationResponse; } }
		public IntentDisambiguation Payload { get; set; }
	}
}
