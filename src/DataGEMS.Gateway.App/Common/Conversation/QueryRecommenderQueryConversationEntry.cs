namespace DataGEMS.Gateway.App.Common.Conversation
{
	public class QueryRecommenderQueryConversationEntry : ConversationEntry
	{
		public override ConversationMessageKind Kind { get { return ConversationMessageKind.CrossDatasetQuery; } }
		public Service.QueryRecommender.RecommenderInfo Payload { get; set; }
	}
}
