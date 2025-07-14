
namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class InDataGeoQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.InDataGeoQuery; } }
		public Service.InDataExploration.ExploreGeoQueryInfo Payload { get; set; }
	}
}
