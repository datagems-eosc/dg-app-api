namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class QueryDisambiguationQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.QueryDisambiguationQuery; } }
		public Service.TaskOrchestrator.DisambiguationInfo Payload { get; set; }
	}
}