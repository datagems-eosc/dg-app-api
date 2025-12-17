namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class QueryRecommenderResponseConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.QueryRecommenderResponse; } }
		public List<DataGEMS.Gateway.App.Model.QueryRecommendation> Payload { get; set; }
	}
}
