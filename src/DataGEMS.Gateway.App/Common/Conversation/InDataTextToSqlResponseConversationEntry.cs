
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataTextToSqlResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataTextToSqlResponse; } }
		public List<DataGEMS.Gateway.App.Model.InDataTextToSqlExploration> Payload { get; set; }
	}
}
