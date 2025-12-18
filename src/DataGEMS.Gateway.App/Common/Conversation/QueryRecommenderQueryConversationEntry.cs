namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class QueryRecommenderQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.QueryRecommenderQuery; } }
		public Service.QueryRecommender.RecommenderInfo Payload { get; set; }
	}
}
