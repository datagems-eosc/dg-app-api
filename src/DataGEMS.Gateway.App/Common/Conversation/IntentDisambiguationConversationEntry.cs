using DataGEMS.Gateway.App.Service.IntentDisambiguation.Model;

namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class IntentDisambiguationConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.IntentDisambiguation; } }
		public IntentDisambiguationRequest Payload { get; set; }
	}
}
