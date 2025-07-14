
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataGeoResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataGeoResponse; } }
		public List<DataGEMS.Gateway.App.Model.InDataGeoQueryExploration> Payload { get; set; }
	}
}
