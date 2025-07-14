
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataTextToSqlQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataTextToSqlQuery; } }
		public Service.InDataExploration.ExploreTextToSqlInfo Payload { get; set; }
	}
}
